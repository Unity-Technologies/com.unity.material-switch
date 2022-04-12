using System.Collections;
using Unity.FilmInternalUtilities.Editor;
using Unity.SelectionGroups;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using UnityEngine.Timeline;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.MaterialSwitch.EditorTests
{
internal class MaterialSwitchClipTests
{
    
    [UnityTest]
    public IEnumerator CopyAndPasteMaterialProperties() {
        
        PlayableDirector director = MaterialSwitchEditorTestUtility.CreateDefaultDirectorAndTrack(
            out TimelineAsset _, out MaterialSwitchTrack track, out SelectionGroup group
        );  
        TimelineEditorUtility.SelectDirectorInTimelineWindow(director);
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(3);

        //Add Sphere and material
        MeshRenderer mr = CreateNewSphereWithMaterialInGroup(group);
        Material mat = mr.sharedMaterial;
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(1);
        
        TimelineClip       clip0   = TimelineEditorReflection.CreateClipOnTrack(typeof(MaterialSwitchClip), track, 0);
        MaterialSwitchClip msClip0 = clip0.asset as MaterialSwitchClip;
        Assert.IsNotNull(msClip0);
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(1);

        //Override
        Color  targetColor         = Color.blue;
        Color  targetEmissionColor = Color.green;
        string colorPropertyName   = "_Color";
        string emissionColorPropertyName   = "_EmissionColor";
        
        VerifyOverrideColor(msClip0, mat, colorPropertyName, targetColor);
        VerifyOverrideColor(msClip0, mat, emissionColorPropertyName, targetEmissionColor);
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(1);

        //Copy and paste
        MaterialPropertiesClipboardData clipboardData = MaterialPropertiesClipboardData.Create(msClip0, 0);

        const int TARGET_MAT_INDEX = 0;
        TimelineClip       clip1   = TimelineEditorReflection.CreateClipOnTrack(typeof(MaterialSwitchClip), track, 0);
        MaterialSwitchClip msClip1 = clip1.asset as MaterialSwitchClip;
        Assert.IsNotNull(msClip1);
        Assert.IsTrue(clipboardData.PasteInto(msClip1,TARGET_MAT_INDEX));
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(1);

        //Check
        VerifyTargetColor(msClip1, TARGET_MAT_INDEX, colorPropertyName, targetColor);
        VerifyTargetColor(msClip1, TARGET_MAT_INDEX, emissionColorPropertyName, targetEmissionColor);
        
    }   

//----------------------------------------------------------------------------------------------------------------------
    
    [UnityTest]
    public IEnumerator CheckMaterialPropertiesCopyAndPasteValidity() {
        
        PlayableDirector director = MaterialSwitchEditorTestUtility.CreateDefaultDirectorAndTrack(
            out TimelineAsset _, out MaterialSwitchTrack track, out SelectionGroup group
        );  
        TimelineEditorUtility.SelectDirectorInTimelineWindow(director);
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(3);

        //Add Sphere and material
        CreateNewSphereWithMaterialInGroup(group);
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(1);
        
        TimelineClip       clip0   = TimelineEditorReflection.CreateClipOnTrack(typeof(MaterialSwitchClip), track, 0);
        MaterialSwitchClip msClip0 = clip0.asset as MaterialSwitchClip;
        Assert.IsNotNull(msClip0);
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(1);

        //Copy and paste
        int numMaterialProperties = msClip0.materialPropertiesList.Count;
        MaterialPropertiesClipboardData validClipboardData = MaterialPropertiesClipboardData.Create(msClip0, 0);
        MaterialPropertiesClipboardData invalidClipboardData = MaterialPropertiesClipboardData.Create(msClip0, numMaterialProperties);

        TimelineClip       clip1   = TimelineEditorReflection.CreateClipOnTrack(typeof(MaterialSwitchClip), track, 0);
        MaterialSwitchClip msClip1 = clip1.asset as MaterialSwitchClip;
        Assert.IsNotNull(msClip1);
        Assert.IsTrue(validClipboardData.PasteInto(msClip1,0));
        Assert.IsFalse(validClipboardData.PasteInto(msClip1,numMaterialProperties));
        Assert.IsFalse(invalidClipboardData.PasteInto(msClip1,0));
        Assert.IsFalse(invalidClipboardData.PasteInto(msClip1,numMaterialProperties));
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(1);
        
    }   
    
//----------------------------------------------------------------------------------------------------------------------

    static void VerifyOverrideColor(MaterialSwitchClip msClip, Material mat, string propertyName, Color targetColor) {
        bool overrideResult = msClip.OverrideProperty(mat, propertyName, targetColor);
        Assert.IsTrue(overrideResult);        
    }

    static void VerifyTargetColor(MaterialSwitchClip msClip, int matIndex, string propertyName, Color color) {
        Assert.AreEqual(color, msClip.materialPropertiesList[matIndex].FindColorProperty(propertyName).GetTargetValue());        
    }

    static MeshRenderer CreateNewSphereWithMaterialInGroup(SelectionGroup group) {
        GameObject   sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        MeshRenderer mr     = sphere.GetComponent<MeshRenderer>();
        Material mat = new Material(mr.sharedMaterial) {
            name = "TestMaterial"
        };
        mr.material = mat;
        group.Add(sphere);
        return mr;
    }
    
}

} //end namespace
