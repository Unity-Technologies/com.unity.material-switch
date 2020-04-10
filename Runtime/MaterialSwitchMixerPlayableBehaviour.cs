using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Playables;

namespace Unity.MaterialSwitch
{
    public partial class MaterialSwitchMixerPlayableBehaviour : PlayableBehaviour
    {
        Material textureLerpMaterial;

        HashSet<RenderTexture> renderTextures = new HashSet<RenderTexture>();
        HashSet<MaterialPropertyBlock> activeMaterialPropertyBlocks = new HashSet<MaterialPropertyBlock>();

        public override void OnPlayableDestroy(Playable playable) {
            foreach(var i in renderTextures) {
                Object.DestroyImmediate(i);
            }
            renderTextures.Clear();
            activeMaterialPropertyBlocks.Clear();
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var group = playerData as SelectionGroups.Runtime.SelectionGroup;
            if (group == null) return;
            var materialGroup = group.GetComponent<MaterialGroup>();
            if (materialGroup == null) return;
            if(Application.isEditor && !Application.isPlaying) {
                materialGroup.CollectMaterials();
            }
            //a group has many renderers, get them all.
            var renderers = group.GetMemberComponents<Renderer>();

            var inputCount = playable.GetInputCount();

            //get total weight of all playables that are currently being mixed.
            var totalWeight = 0f;
            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                totalWeight += weight;
            }
            //weights should add up to 1.0, therefore calculate any missing weight using 1 - total.
            var missingWeight = 1f - totalWeight;

            //there is nothing to do (missing weight = 1 or total weight = 0) remove any property blocks then exit.
            if (missingWeight >= 1f)
            {
                RemoveMaterialPropertyBlocks(renderers);
                return;
            }

            //make sure renderers have property blocks so we can adjust the material properties.
            AssignMaterialPropertyBlocks(materialGroup, renderers);
            //get materials from each renderer, then match to a palettePropertyMap to assign values.
            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                if (weight == 0) continue;

                var paletteSwitchBehaviour = ((ScriptPlayable<MaterialSwitchPlayableBehaviour>)playable.GetInput(i)).GetBehaviour();

                //each renderer in the group can have many materials.
                //calculate the colors and textures coming from this clip and update relevant property block
                foreach (var renderer in renderers)
                {
                    for (var index = 0; index < renderer.sharedMaterials.Length; index++)
                    {
                        var material = renderer.sharedMaterials[index];

                        //get the matching property map from this clip for a material.
                        //the property map contains properties that are added into the appropriate property block.
                        var ppm = paletteSwitchBehaviour.GetMap(material);

                        var mpb = materialGroup.GetMaterialPropertyBlock(material);

                        //if the block is empty, populate it with base values from the original material so they can be lerped towards target values.
                        if (!activeMaterialPropertyBlocks.Contains(mpb)) {
                            InitPropertyBlock(ppm, mpb);
                            activeMaterialPropertyBlocks.Add(mpb);
                        }

                        LerpCurrentColorsToTargetColors(weight, ppm, mpb);
                        LerpCurrentTexturesToTargetTextures(weight, ppm, mpb);
                        renderer.SetPropertyBlock(mpb, index);
                    }
                }
            }

        }

        void LerpCurrentTexturesToTargetTextures(float weight, PalettePropertyMap ppm, MaterialPropertyBlock mpb)
        {
            // the material property block contains "final" textures which are used in rendering.
            // each textureProperty in the palette property map also has a reference to the final texture.
            // the palette property map contains textures which the user may have changed.
            // at the end of this function, each texture in the property block should have changes based on the weight and user textures.
            foreach (var tp in ppm.textureProperties)
            {
                var finalTex = tp.finalTexture;
                if (finalTex != null)
                {
                    //copy finalTex state then blend and assign new state.
                    var finalTexCopy = RenderTexture.GetTemporary(finalTex.width, finalTex.height);
                    Graphics.Blit(finalTex, finalTexCopy);
                    //setup blit parameters for texture lerp. if there is no target to lerp towards, lerp back to original.
                    if (textureLerpMaterial == null)
                        textureLerpMaterial = CreateTextureLerpMaterial();
                    textureLerpMaterial.SetFloat("_Weight", weight);
                    if (tp.targetValue == null)
                        textureLerpMaterial.SetTexture("_TargetTex", tp.originalValue);
                    else
                        textureLerpMaterial.SetTexture("_TargetTex", tp.targetValue);
                    //finally interpolate textures and update the final texture.
                    Graphics.Blit(finalTexCopy, finalTex, textureLerpMaterial);
                    RenderTexture.ReleaseTemporary(finalTexCopy);
                }
            }
        }

        static void LerpCurrentColorsToTargetColors(float weight, PalettePropertyMap ppm, MaterialPropertyBlock mpb)
        {
            //lerp the colors towards targets.
            foreach (var cc in ppm.colorCoordinates)
            {
                var color = mpb.GetColor(cc._propertyName);
                color = Color.Lerp(color, cc.sampledColor, weight);
                mpb.SetColor(cc._propertyName, color);
            }
        }

        void InitPropertyBlock(PalettePropertyMap ppm, MaterialPropertyBlock mpb)
        {
            //colors
            foreach (var cc in ppm.colorCoordinates)
            {
                mpb.SetColor(cc._propertyName, cc.originalColor);
            }
            //textures
            foreach (var tp in ppm.textureProperties)
            {
                if (tp.originalValue != null)
                {
                    //this is the texture to which all clips can contribute.
                    var finalTexture = mpb.GetTexture(tp.propertyId);
                    //if it has not been created on this property block, do it now.
                    if (finalTexture == null)
                    {
                        var texture = tp.originalValue;
                        finalTexture = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
                        renderTextures.Add((RenderTexture)finalTexture);
                        //copy original color values into the new texture.
                        Graphics.Blit(texture, (RenderTexture)finalTexture);
                        mpb.SetTexture(tp.propertyId, finalTexture);
                    }
                    //store a reference to the final texture in the texture property.
                    tp.finalTexture = (RenderTexture)finalTexture;
                }
            }
        }

        static Material CreateTextureLerpMaterial()
        {
            var m = new Material(Shader.Find("Hidden/TextureLerp"));
            return m;
        }

        static void AssignMaterialPropertyBlocks(MaterialGroup group, IEnumerable<Renderer> renderers)
        {
            foreach (var r in renderers)
            {
                for (var i = 0; i < r.sharedMaterials.Length; i++)
                {
                    var mpb = group.GetMaterialPropertyBlock(r.sharedMaterials[i]);
                    mpb.Clear();
                    r.SetPropertyBlock(mpb, i);
                }
            }
        }

        static void RemoveMaterialPropertyBlocks(IEnumerable<Renderer> renderers)
        {
            foreach (var r in renderers)
            {
                for (var i = 0; i < r.sharedMaterials.Length; i++)
                {
                    r.SetPropertyBlock(null, i);
                }
            }
            return;
        }
    }
}