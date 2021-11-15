using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    public static class MaterialSwitchUtility
    {
        [InitializeOnLoadMethod]
        static void InitCallbacks()
        {
            MaterialSwitchPlayableBehaviour.CreateMaterialProperties = CreateMaterialProperties;
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