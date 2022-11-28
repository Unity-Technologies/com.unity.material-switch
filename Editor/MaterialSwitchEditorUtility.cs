using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitch {
/// <summary>
/// Utility class to perform various functions on MaterialSwitch classes.
/// </summary>
public static class MaterialSwitchEditorUtility {
    
    private static Dictionary<Shader,MaterialPropertyNameMap> _nameRemaps;

    [InitializeOnLoadMethod]
    static void InitCallbacks() {
        MaterialSwitchPlayableBehaviour.CreateMaterialProperties = CreateMaterialProperties;
    }

    /// <summary>
    /// Init a MaterialSwitchClip 
    /// </summary>
    /// <param name="clip">The clip to be initialized</param>
    [CanBeNull]
    public static MaterialSwitchClip InitMaterialSwitchClip(TimelineClip clip, TrackAsset track) {
        PlayableDirector inspectedDirector = TimelineEditor.inspectedDirector;
        if (null == inspectedDirector)
            return null;

        SelectionGroup selectionGroup = inspectedDirector.GetGenericBinding(track) as SelectionGroup;
        if (selectionGroup == null)
            return null;

        if (!selectionGroup.TryGetComponent<MaterialGroup>(out MaterialGroup materialPropertyGroup)) {
            materialPropertyGroup = selectionGroup.gameObject.AddComponent<MaterialGroup>();
        }

        Assert.IsNotNull(materialPropertyGroup);

        MaterialSwitchClip playableAsset = clip.asset as MaterialSwitchClip;
        if (null == playableAsset) {
            Debug.LogError("Asset is not a MaterialSwitchClip: " + clip.asset);
            return null;
        }

        if (playableAsset.materialPropertiesList != null) {
            //This should be ok, probably from a duplicate operation.
            //Debug.LogError("PalettePropertyMap is already created.");
            return playableAsset;
        }

        playableAsset.globalMaterialProperties = CreateMaterialProperties(materialPropertyGroup.sharedMaterials);
        playableAsset.materialPropertiesList =
            new List<MaterialProperties>(materialPropertyGroup.sharedMaterials.Length);
        foreach (Material t in materialPropertyGroup.sharedMaterials) {
            MaterialProperties ppm = CreateMaterialProperties(t);
            playableAsset.materialPropertiesList.Add(ppm);
        }

        return playableAsset;
    }


    internal static MaterialProperties CreateMaterialProperties(Material material) {
        var map = CreateMaterialProperties(new[] { material });
        map.material = material;
        return map;
    }

    internal static string GetDisplayName(Material material, string propertyName)
    {
        if(material == null || material.shader == null || _nameRemaps == null)
            return propertyName;
        if (_nameRemaps.TryGetValue(material.shader, out var nameMap))
        {
            if (nameMap.TryGetValue(propertyName, out var remappedName))
            {
                return remappedName.displayName;
            }
        }
        return propertyName;
    }
    
    internal static MaterialProperties CreateMaterialProperties(Material[] materials) {
        var mapAssets = Resources.FindObjectsOfTypeAll<MaterialPropertyNameMap>();
        _nameRemaps = new Dictionary<Shader, MaterialPropertyNameMap>();
        foreach (var i in mapAssets)
            if(i != null && i.shader != null) _nameRemaps[i.shader] = i;
        
        MaterialProperties ppm = new MaterialProperties() {
            needsUpdate = false,
        };
        var materialProperties = new List<(Material, MaterialProperty)>();

        foreach (var i in materials) {
            var mps = MaterialEditor.GetMaterialProperties(new[] { i });
            if (mps == null) continue;
            materialProperties.AddRange(from j in mps select (i,j));
        }

        if (materialProperties.Count == 0)
            return ppm;

        foreach (var (material, mp) in materialProperties) {
            if (mp.flags.HasFlag(MaterialProperty.PropFlags.HideInInspector))
                continue;
            if (mp.flags.HasFlag(MaterialProperty.PropFlags.PerRendererData))
                continue;
            var displayName = mp.displayName;
            if (material.shader != null)
            {
                if (_nameRemaps.TryGetValue(material.shader, out var map))
                {
                    if (map.TryGetValue(mp.name, out var propertyName))
                    {
                        if (propertyName.hidden) continue;
                        displayName = propertyName.displayName;
                    }
                }
            }

            if (mp.type == MaterialProperty.PropType.Color) {
                ppm.colorProperties.Add(
                    new ColorProperty() {
                        uv           = Vector2.zero,
                        displayName  = displayName,
                        propertyName = mp.name,
                        targetValue  = Color.clear,
                        baseValue    = mp.colorValue
                    }
                );
            }

            if (mp.type == MaterialProperty.PropType.Texture) {
                ppm.textureProperties.Add(
                    new TextureProperty() {
                        displayName  = displayName,
                        propertyName = mp.name,
                        propertyId   = Shader.PropertyToID(mp.name),
                        baseValue    = (Texture2D)mp.textureValue
                    }
                );
            }

            if (mp.type == MaterialProperty.PropType.Float) {
                ppm.floatProperties.Add(
                    new FloatProperty() {
                        displayName  = displayName,
                        propertyName = mp.name,
                        propertyId   = Shader.PropertyToID(mp.name),
                        baseValue    = mp.floatValue,
                        targetValue  = mp.floatValue
                    }
                );
            }

            if (mp.type == MaterialProperty.PropType.Range) {
                ppm.floatProperties.Add(
                    new RangeProperty() {
                        displayName  = displayName,
                        propertyName = mp.name,
                        propertyId   = Shader.PropertyToID(mp.name),
                        baseValue    = mp.floatValue,
                        targetValue  = mp.floatValue,
                        rangeLimits  = mp.rangeLimits
                    }
                );
            }
        }

        return ppm;
    }
}
} //end namespace