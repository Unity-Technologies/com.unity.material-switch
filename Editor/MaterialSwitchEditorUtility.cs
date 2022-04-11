﻿using System.Collections.Generic;
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

    internal static MaterialProperties CreateMaterialProperties(Material[] materials) {
        MaterialProperties ppm = new MaterialProperties() {
            needsUpdate = false,
        };
        var materialProperties = new List<MaterialProperty>();

        foreach (var i in materials) {
            var mps = MaterialEditor.GetMaterialProperties(new[] { i });
            if (mps == null) continue;
            materialProperties.AddRange(mps);
        }

        if (materialProperties.Count == 0)
            return ppm;

        foreach (var mp in materialProperties) {
            if (mp.flags.HasFlag(MaterialProperty.PropFlags.HideInInspector))
                continue;
            if (mp.flags.HasFlag(MaterialProperty.PropFlags.PerRendererData))
                continue;

            if (mp.type == MaterialProperty.PropType.Color) {
                ppm.colorProperties.Add(
                    new ColorProperty() {
                        uv           = Vector2.zero,
                        displayName  = mp.displayName,
                        propertyName = mp.name,
                        targetValue  = Color.clear,
                        baseValue    = mp.colorValue
                    }
                );
            }

            if (mp.type == MaterialProperty.PropType.Texture) {
                ppm.textureProperties.Add(
                    new TextureProperty() {
                        displayName  = mp.displayName,
                        propertyName = mp.name,
                        propertyId   = Shader.PropertyToID(mp.name),
                        baseValue    = (Texture2D)mp.textureValue
                    }
                );
            }

            if (mp.type == MaterialProperty.PropType.Float) {
                ppm.floatProperties.Add(
                    new FloatProperty() {
                        displayName  = mp.displayName,
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
                        displayName  = mp.displayName,
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
    
//----------------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Copy the material properties of MaterialSwitchClip 
        /// </summary>
        /// <param name="src">The MaterialSwitchClip source</param>
        /// <param name="materialIndex">The index of the material in the clip. Index less than 0 means global properties
        /// </param>
        /// <returns></returns>
        public static void CopyMaterialSwitchClipMaterialProperties(MaterialSwitchClip src, int materialIndex) {
            Assert.IsNotNull(src);
            m_copyClipSource  = Object.Instantiate(src) as MaterialSwitchClip;
            m_copyClipSourceMaterialIndex = materialIndex;
        }

        /// <summary>
        /// Checks if MaterialSwitchClip material properties have been copied and ready to be pasted.  
        /// </summary>
        /// <returns>true if pasting is possible, false otherwise</returns>
        public static bool CanPasteMaterialSwitchClipMaterialProperties() {
            return (null != m_copyClipSource);
        }
        
        /// <summary>
        /// Paste copied material properties of MaterialSwitchClip to destination clip.  
        /// </summary>
        /// <param name="target">The MaterialSwitchClip target</param>
        /// <param name="targetMaterialIndex">
        ///     The index of the material in the target clip.
        ///     Index less than 0 means global properties.
        /// </param>
        /// <returns></returns>
        public static void PasteMaterialSwitchClipMaterialProperties(MaterialSwitchClip target, 
            int targetMaterialIndex) 
        {
            
            Undo.RecordObject(target, "Paste");
            //negative targetIndex is reserved for global properties
            if (targetMaterialIndex < 0)
            {
                string json = ConvertCopiedClipMaterialPropertiesToJson();
                EditorJsonUtility.FromJsonOverwrite(json, target.globalMaterialProperties);
            }
            else
            {
                //preserve material reference, this is not normally changed.
                var    oldMaterial = target.materialPropertiesList[targetMaterialIndex].material;
                string json        = ConvertCopiedClipMaterialPropertiesToJson();

                EditorJsonUtility.FromJsonOverwrite(json, target.materialPropertiesList[targetMaterialIndex]);
                target.materialPropertiesList[targetMaterialIndex].material = oldMaterial;
            }
                    
            //serializedObject.ApplyModifiedProperties();
        }

        static string ConvertCopiedClipMaterialPropertiesToJson() {
            if (m_copyClipSourceMaterialIndex < 0)
                return EditorJsonUtility.ToJson(m_copyClipSource.globalMaterialProperties);
            
            return EditorJsonUtility.ToJson(m_copyClipSource.materialPropertiesList[m_copyClipSourceMaterialIndex]);
        }

//----------------------------------------------------------------------------------------------------------------------
        
        private static MaterialSwitchClip m_copyClipSource;
        private static int                m_copyClipSourceMaterialIndex; // < 0 means Global    
}
} //end namespace