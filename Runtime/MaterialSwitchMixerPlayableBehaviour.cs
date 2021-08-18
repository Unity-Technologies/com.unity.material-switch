using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.MaterialSwitch
{
    internal class MaterialSwitchMixerPlayableBehaviour : PlayableBehaviour
    {
        Material textureLerpMaterial;

        HashSet<Renderer> renderers;

        HashSet<PalettePropertyMap> activePalettePropertyMapInstances= new HashSet<PalettePropertyMap>();

        public override void OnPlayableDestroy(Playable playable)
        {
            RemoveMaterialPropertyBlocks();
            foreach(var ppm in activePalettePropertyMapInstances) {
                foreach(var i in ppm.textureProperties) {
                    if(i.finalTexture != null) {
                        i.finalTexture.Release();
                        i.finalTexture = null;
                    }
                }
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var group = playerData as SelectionGroups.Runtime.SelectionGroup;
            if (group == null) return;
            var materialGroup = group.GetComponent<MaterialGroup>();
            if (materialGroup == null) return;
            if (Application.isEditor && !Application.isPlaying)
            {
                materialGroup.CollectMaterials();
                
            }
            //a group has many renderers, get them all.
            if (renderers == null)
                renderers = new HashSet<Renderer>(group.GetMemberComponents<Renderer>());
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
                RemoveMaterialPropertyBlocks();
                return;
            }

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
                        var map = paletteSwitchBehaviour.GetMap(material);
                        if (map == null)
                        {
                            //this will happen when a material is added or removed after the clip has been created in the timeline. Cannot avoid this for now.
                            continue;
                        }
                        
                        var block = new MaterialPropertyBlock();
                        renderer.GetPropertyBlock(block, index);
                        var globalTexture = paletteSwitchBehaviour.GetGlobalTexture();
                        LerpCurrentColorsToTargetColors(globalTexture, weight, map, block);
                        LerpCurrentTexturesToTargetTextures(weight, map, block);
                        LerpCurrentFloatsToTargetFloats(weight, map, block);

                        renderer.SetPropertyBlock(block, index);
                    }
                }
            }

        }

        void LerpCurrentTexturesToTargetTextures(float weight, PalettePropertyMap map, MaterialPropertyBlock block)
        {
            // the material property block contains "final" textures which are used in rendering.
            // each textureProperty in the palette property map also has a reference to the final texture.
            // the palette property map contains textures which the user may have changed.
            // at the end of this function, each texture in the property block should have changes based on the weight and user textures.
            activePalettePropertyMapInstances.Add(map);
            foreach (var i in map.textureProperties)
            {
                if (i.overrideBaseValue && i.baseValue != null)
                {
                    if (i.finalTexture == null)
                        i.finalTexture = new RenderTexture(i.baseValue.width, i.baseValue.height, 0, RenderTextureFormat.ARGB32);
                    //setup blit parameters for texture lerp. if there is no target to lerp towards, lerp back to original.
                    if (textureLerpMaterial == null)
                        textureLerpMaterial = CreateTextureLerpMaterial();
                    textureLerpMaterial.SetFloat("_Weight", weight);
                    if (i.targetValue == null)
                        textureLerpMaterial.SetTexture("_TargetTex", i.baseValue);
                    else
                        textureLerpMaterial.SetTexture("_TargetTex", i.targetValue);
                    //finally interpolate textures and update the final texture.
                    Graphics.Blit(i.baseValue, i.finalTexture, textureLerpMaterial);
                    block.SetTexture(i.propertyName, i.finalTexture);
                }
            }
        }

        static void LerpCurrentColorsToTargetColors(Texture2D globalTexture, float weight, PalettePropertyMap map,
            MaterialPropertyBlock block)
        {
            //if palette texture is set to null, don't lerp the colors.
            if (globalTexture == null && map.texture == null) return;
            //lerp the colors towards targets.
            foreach (var i in map.colorCoordinates)
            {
                if (i.overrideBaseValue)
                {
                    var color = Color.Lerp(i.baseValue, i.targetValue, weight);
                    block.SetColor(i.propertyName, color);
                }
            }
        }

        static void LerpCurrentFloatsToTargetFloats(float weight, PalettePropertyMap map, MaterialPropertyBlock block)
        {
            //lerp the colors towards targets.
            foreach (var i in map.floatProperties)
            {
                if (i.overrideBaseValue)
                {
                    var v = Mathf.Lerp(i.baseValue, i.targetValue, weight);
                    block.SetFloat(i.propertyName, v);
                }
            }
        }

        static Material CreateTextureLerpMaterial()
        {
            var m = new Material(Shader.Find("Hidden/TextureLerp"));
            return m;
        }

        void RemoveMaterialPropertyBlocks()
        {
            if (renderers != null)
                foreach (var r in renderers)
                {
                    if (r == null) continue;
                    for (var i = 0; i < r.sharedMaterials.Length; i++)
                    {
                        r.SetPropertyBlock(null, i);
                    }
                }
        }
    }
}