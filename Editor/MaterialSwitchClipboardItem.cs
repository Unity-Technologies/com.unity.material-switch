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

    internal MaterialPropertiesClipboardData(MaterialSwitchClip clip, int matIndex) {
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
    /// <returns></returns>
    public void PasteInto(MaterialSwitchClip target, int targetMaterialIndex) 
    {
            
        Undo.RecordObject(target, "Paste");
        //negative targetIndex is reserved for global properties
        if (targetMaterialIndex < 0)
        {
            string json = ConvertClipboardDataToJson(this);
            EditorJsonUtility.FromJsonOverwrite(json, target.globalMaterialProperties);
        }
        else
        {
            //preserve material reference, this is not normally changed.
            var    oldMaterial = target.materialPropertiesList[targetMaterialIndex].material;
            string json        = ConvertClipboardDataToJson(this);

            EditorJsonUtility.FromJsonOverwrite(json, target.materialPropertiesList[targetMaterialIndex]);
            target.materialPropertiesList[targetMaterialIndex].material = oldMaterial;
        }
                    
        //serializedObject.ApplyModifiedProperties();
    }
    
    static string ConvertClipboardDataToJson(MaterialPropertiesClipboardData clipboardData) {
        int                matIndex = clipboardData.m_materialIndex;
        MaterialSwitchClip clip     = clipboardData.m_clip;
        if (matIndex < 0)
            return EditorJsonUtility.ToJson(clip.globalMaterialProperties);
            
        return EditorJsonUtility.ToJson(clip.materialPropertiesList[matIndex]);
    }    

//----------------------------------------------------------------------------------------------------------------------

    readonly MaterialSwitchClip m_clip;
    readonly int                m_materialIndex;

    
}

} //end namespace