using System.Collections;
using NUnit.Framework;
using Unity.FilmInternalUtilities;
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
        GameObject   sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        MeshRenderer mr     = sphere.GetComponent<MeshRenderer>();
        Material     mat    = new Material(mr.sharedMaterial) {
            name = "TestMaterial"
        };
        mr.material = mat;        
        group.Add(sphere);
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(1);
        
        TimelineClip       clip0   = TimelineEditorReflection.CreateClipOnTrack(typeof(MaterialSwitchClip), track, 0);
        MaterialSwitchClip msClip0 = clip0.asset as MaterialSwitchClip;
        Assert.IsNotNull(msClip0);
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(1);

        //Override
        Color targetColor = Color.blue;
        Color targetEmissionColor = Color.green;

        bool overrideResult = msClip0.OverrideProperty(mat, "_Color", targetColor);
        Assert.IsTrue(overrideResult);
        
        ColorProperty colorProp = msClip0.materialPropertiesList[0].FindColorProperty("_Color");
        colorProp.overrideBaseValue
        
        // foreach (var a in msClip0.GetMaterialProperties()) {
        //     foreach (var b in a.GetColorProperties()) {
        //         Debug.Log(a.material.name + " " + b.propertyName);
        //         
        //     }
        // }
        
        
        TimelineClip       clip1   = TimelineEditorReflection.CreateClipOnTrack(typeof(MaterialSwitchClip), track, 0);
        MaterialSwitchClip msClip1 = clip1.asset as MaterialSwitchClip; 
        Assert.IsNotNull(msClip1);
        
    }   
    
//----------------------------------------------------------------------------------------------------------------------    
    
}

} //end namespace
