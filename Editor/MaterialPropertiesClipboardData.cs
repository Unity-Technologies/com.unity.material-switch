using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.MaterialSwitch
{

/// <summary>
/// Clipboard data that is used for copy/paste material properties of MaterialSwitchClip
/// </summary>
public class MaterialPropertiesClipboardData
{    

    MaterialPropertiesClipboardData(MaterialSwitchClip clip, int matIndex) {
        m_clip          = clip;
        m_materialIndex = matIndex;
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    /// <summary>
    /// Create material properties clipboard data of MaterialSwitchClip that can be pasted on another MaterialSwitchClip  
    /// </summary>
    /// <param name="src">The MaterialSwitchClip source</param>
    /// <param name="materialIndex">The index of the material in the clip. Index less than 0 means global properties
    /// </param>
    /// <returns>Material properties data that can be pasted on another MaterialSwitchClip</returns>
    [NotNull]
    public static MaterialPropertiesClipboardData Create(MaterialSwitchClip src, int materialIndex) {
        Assert.IsNotNull(src);
        return new MaterialPropertiesClipboardData(Object.Instantiate(src), materialIndex);
    }
    
    /// <summary>
    /// Paste this MaterialPropertiesClipboardData into a certain material in the target MaterialSwitchClip.  
    /// </summary>
    /// <param name="target">The MaterialSwitchClip target</param>
    /// <param name="targetMaterialIndex">
    ///     The index of the material in the target clip.
    ///     Index less than 0 means global properties.
    /// </param>
    /// <returns>true if the paste is successful, false otherwise</returns>
    public bool PasteInto(MaterialSwitchClip target, int targetMaterialIndex) {
            
        Undo.RecordObject(target, "Paste");
        string json = ConvertClipboardDataToJson(this);
        if (null == json)
            return false;
        
        //negative targetIndex is reserved for global properties
        if (IsGlobalMaterialProperty(targetMaterialIndex)) {
            EditorJsonUtility.FromJsonOverwrite(json, target.globalMaterialProperties);
            return true;
        } 

        if (targetMaterialIndex >= target.materialPropertiesList.Count)
            return false;
        
        //preserve material reference, this is not normally changed.
        Material  oldMaterial = target.materialPropertiesList[targetMaterialIndex].material;

        EditorJsonUtility.FromJsonOverwrite(json, target.materialPropertiesList[targetMaterialIndex]);
        target.materialPropertiesList[targetMaterialIndex].material = oldMaterial;
        return true;
    }

    [CanBeNull]
    static string ConvertClipboardDataToJson(MaterialPropertiesClipboardData clipboardData) {
        int                matIndex = clipboardData.m_materialIndex;
        MaterialSwitchClip clip     = clipboardData.m_clip;
        if (IsGlobalMaterialProperty(matIndex))
            return EditorJsonUtility.ToJson(clip.globalMaterialProperties);

        if (matIndex >= clip.materialPropertiesList.Count)
            return null;
        
        return EditorJsonUtility.ToJson(clip.materialPropertiesList[matIndex]);
    }

    static bool IsGlobalMaterialProperty(int index) {
        return index < 0;
    }

//----------------------------------------------------------------------------------------------------------------------

    readonly MaterialSwitchClip m_clip;
    readonly int                m_materialIndex;

    
}

} //end namespace