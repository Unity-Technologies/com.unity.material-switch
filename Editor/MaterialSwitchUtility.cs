using UnityEditor;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    public static class MaterialSwitchUtility
    {
        [InitializeOnLoadMethod]
        static void InitCallbacks()
        {
            MaterialSwitchPlayableBehaviour.CreatePalettePropertyMap = InitPalettePropertyMap;
        }

        internal static PalettePropertyMap InitPalettePropertyMap(Material material)
        {
            var map = InitPalettePropertyMap(new[] {material});
            map.material = material;
            return map;
        }
        
        internal static PalettePropertyMap InitPalettePropertyMap(Material[] materials)
        {
            PalettePropertyMap ppm = new PalettePropertyMap() 
            {
                needsUpdate = false,
            };
            
            MaterialProperty[] materialProperties = MaterialEditor.GetMaterialProperties(materials);
            if (null == materialProperties)
                return ppm;
            
            foreach (var mp in materialProperties)
            {
                if (mp.flags.HasFlag(MaterialProperty.PropFlags.HideInInspector))
                    continue;
                if (mp.flags.HasFlag(MaterialProperty.PropFlags.PerRendererData))
                    continue;

                if (mp.type == MaterialProperty.PropType.Color)
                {
                    ppm.colorCoordinates.Add(
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