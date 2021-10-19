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
            if (textureLerpMaterial == null)
                textureLerpMaterial = CreateTextureLerpMaterial();
            
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
                
                
                // //each renderer in the group can have many materials.
                // //calculate the colors and textures coming from this clip and update relevant property block
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
                        
                        foreach (var textureProperty in map.textureProperties)
                        {
                            if (textureProperty.overrideBaseValue)
                            {
                                if (block.GetTexture(textureProperty.propertyName) == null)
                                {
                                    var finalTexture = new RenderTexture(textureProperty.baseValue.width, textureProperty.baseValue.height, 0, RenderTextureFormat.ARGB32);
                                    block.SetTexture(textureProperty.propertyName, finalTexture);
                                    RenderTexture.active = finalTexture;
                                    GL.Clear(true, false, Color.black);
                                    RenderTexture.active = null;
                                }
                            }
                        }
                        
                        var globalMap = paletteSwitchBehaviour.GetGlobalMap();
                        LerpCurrentColorsToTargetColors(weight, globalMap, block);
                        LerpCurrentTexturesToTargetTextures(weight, globalMap, block);
                        LerpCurrentFloatsToTargetFloats(weight, globalMap, block);
                        
                        LerpCurrentColorsToTargetColors(weight, map, block);
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

            // foreach (var textureProperty in map.textureProperties)
            // {
            //     if (textureProperty.overrideBaseValue)
            //     {
            //         var finalTexture = (RenderTexture) block.GetTexture(textureProperty.propertyName);
            //         if (finalTexture == null)
            //         {
            //             finalTexture = new RenderTexture(textureProperty.baseValue.width,
            //                 textureProperty.baseValue.height, 0, RenderTextureFormat.ARGB32);
            //             RenderTexture.active = finalTexture;
            //             // GL.Clear(true, false, Color.black);
            //             RenderTexture.active = null;
            //             block.SetTexture(textureProperty.propertyName, finalTexture);
            //         }
            //     }
            // }

            
            foreach (var textureProperty in map.textureProperties)
            {
                if (textureProperty.overrideBaseValue)
                {
                    var finalTexture = (RenderTexture) block.GetTexture(textureProperty.propertyName);
                    //setup blit parameters for texture lerp. if there is no target to lerp towards, lerp back to original.
                    textureLerpMaterial.SetFloat("_Weight", weight);
                    textureLerpMaterial.SetTexture("_TargetTex", textureProperty.targetValue);
                    //finally interpolate textures and update the final texture.
                    var output = RenderTexture.GetTemporary(finalTexture.descriptor);
                    Graphics.Blit(finalTexture, output, textureLerpMaterial);
                    Graphics.Blit(output, finalTexture);
                    RenderTexture.ReleaseTemporary(output);

                }
            }
        }

        static void LerpCurrentColorsToTargetColors(float weight, PalettePropertyMap map,
            MaterialPropertyBlock block)
        {
            //lerp the colors towards targets.
            foreach (var i in map.colorCoordinates)
            {
                if (i.overrideBaseValue)
                {
                    var color = block.GetColor(i.propertyName);
                    //var color = Color.Lerp(i.baseValue, i.targetValue, weight);
                    block.SetColor(i.propertyName, color + i.targetValue * weight );
                }
            }
        }

        static void LerpCurrentFloatsToTargetFloats(float weight, PalettePropertyMap map, MaterialPropertyBlock block)
        {
            //lerp the floats towards targets.
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