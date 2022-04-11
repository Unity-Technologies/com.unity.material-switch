namespace Unity.MaterialSwitch
{

/// <summary>
/// Clipboard data that is used for copy/paste material properties of MaterialSwitchClip
/// </summary>
public class MaterialPropertiesClipboardData
{    
    internal MaterialSwitchClip clip;
    internal int                materialIndex;

    internal MaterialPropertiesClipboardData(MaterialSwitchClip c, int matIndex) {
        clip          = c;
        materialIndex = matIndex;
    }
    
}

} //end namespace