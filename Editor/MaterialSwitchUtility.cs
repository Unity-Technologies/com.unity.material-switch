using System.Collections.Generic;
using NUnit.Framework;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitch
{
    /// <summary>
    /// Utility class to perform various functions on MaterialSwitch classes.
    /// </summary>
    public static class MaterialSwitchUtility
    {
        [InitializeOnLoadMethod]
        static void InitCallbacks()
        {
            MaterialSwitchPlayableBehaviour.CreateMaterialProperties = CreateMaterialProperties;
        }

        /// <summary>
        /// Init a MaterialSwitchClip 
        /// </summary>
        /// <param name="clip">The clip to be initialized</param>
        public static void InitMaterialSwitchClip(TimelineClip clip) {
            
            TrackAsset track = clip.GetParentTrack();
            SelectionGroup selectionGroup = TimelineEditor.inspectedDirector.GetGenericBinding(track) as SelectionGroup;
            if (selectionGroup == null)
                return;
            
            if (!selectionGroup.TryGetComponent<MaterialGroup>(out MaterialGroup materialPropertyGroup))
            {
                materialPropertyGroup = selectionGroup.gameObject.AddComponent<MaterialGroup>();
            }
            Assert.IsNotNull(materialPropertyGroup);
            
            MaterialSwitchClip playableAsset = clip.asset as MaterialSwitchClip;             
            if (null == playableAsset)
            {
                Debug.LogError("Asset is not a PaletteSwitchClip: " + clip.asset);
                return;
            }
            
            if (playableAsset.materialPropertiesList != null)
            {
                //This should be ok, probably from a duplicate operation.                
                //Debug.LogError("PalettePropertyMap is already created.");                
                return;
            }

            playableAsset.globalMaterialProperties = CreateMaterialProperties(materialPropertyGroup.sharedMaterials);
            playableAsset.materialPropertiesList   = new List<MaterialProperties>(materialPropertyGroup.sharedMaterials.Length);
            foreach (Material t in materialPropertyGroup.sharedMaterials) {
                MaterialProperties ppm = CreateMaterialProperties(t);
                playableAsset.materialPropertiesList.Add(ppm);
            }
            
        }
        

        internal static MaterialProperties CreateMaterialProperties(Material material)
        {
            var map = CreateMaterialProperties(new[] {material});
            map.material = material;
            return map;
        }
        
        internal static MaterialProperties CreateMaterialProperties(Material[] materials)
        {
            MaterialProperties ppm = new MaterialProperties() 
            {
                needsUpdate = false,
            };
            var materialProperties = new List<MaterialProperty>();

            foreach (var i in materials)
            {
                var mps = MaterialEditor.GetMaterialProperties(new[] {i});
                if (mps == null) continue;
                materialProperties.AddRange(mps);
            }
            
            if (materialProperties.Count == 0)
                return ppm;
            
            foreach (var mp in materialProperties)
            {
                if (mp.flags.HasFlag(MaterialProperty.PropFlags.HideInInspector))
                    continue;
                if (mp.flags.HasFlag(MaterialProperty.PropFlags.PerRendererData))
                    continue;

                if (mp.type == MaterialProperty.PropType.Color)
                {
                    ppm.colorProperties.Add(
                        new ColorProperty()
                        {
                            uv = Vector2.zero,
                            displayName = mp.displayName,
                            propertyName = mp.name,
                            targetValue = Color.clear,
                            baseValue = mp.colorValue
                        }
                    );
                }

                if (mp.type == MaterialProperty.PropType.Texture)
                {
                    ppm.textureProperties.Add(
                        new TextureProperty()
                        {
                            displayName = mp.displayName,
                            propertyName = mp.name,
                            propertyId = Shader.PropertyToID(mp.name),
                            baseValue = (Texture2D) mp.textureValue
                        }
                    );
                }

                if (mp.type == MaterialProperty.PropType.Float)
                {
                    ppm.floatProperties.Add(
                        new FloatProperty()
                        {
                            displayName = mp.displayName,
                            propertyName = mp.name,
                            propertyId = Shader.PropertyToID(mp.name),
                            baseValue = mp.floatValue,
                            targetValue = mp.floatValue
                        }
                    );
                }

                if (mp.type == MaterialProperty.PropType.Range)
                {
                    ppm.floatProperties.Add(
                        new RangeProperty()
                        {
                            displayName = mp.displayName,
                            propertyName = mp.name,
                            propertyId = Shader.PropertyToID(mp.name),
                            baseValue = mp.floatValue,
                            targetValue = mp.floatValue,
                            rangeLimits = mp.rangeLimits
                        }
                    );
                }
            }

            return ppm;
        }
    }
}