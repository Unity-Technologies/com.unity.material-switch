using System.Collections;
using NUnit.Framework;
using Unity.FilmInternalUtilities.Editor;
using Unity.SelectionGroups;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using UnityEngine.Timeline;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.MaterialSwitch.EditorTests
{
internal class MaterialSwitchTrackTests
{
//----------------------------------------------------------------------------------------------------------------------    

    [UnityTest]
    public IEnumerator CreateEmptyPlayableAsset() {
        
        PlayableDirector director = MaterialSwitchEditorTestUtility.CreateDirectorWithTimelineAsset(
            out TimelineAsset timelineAsset
        );  
        
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(3);        
        TimelineEditorUtility.CreateTrackAndClip<MaterialSwitchTrack, MaterialSwitchClip>(timelineAsset, "TestTrack");
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(3);

    }

//----------------------------------------------------------------------------------------------------------------------
    //[TODO-sin: 2021-11-10] Include this test as well
    [Ignore("CreateClip")]
    [UnityTest]
    public IEnumerator CreateClip() {
        TimelineAsset timelineAsset = TimelineEditorUtility.CreateAsset(MaterialSwitchTestEditorConstants.TEST_TIMELINE_ASSET_PATH);
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(3);
        
        TimelineClip clip = TimelineEditorUtility.CreateTrackAndClip(timelineAsset, "TestTrack",
            typeof(MaterialSwitchTrack), typeof(MaterialSwitchClip));
        
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(3);

        TimelineEditorUtility.DestroyAssets(clip);
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    [UnityTest]
    public IEnumerator AssignSelectionGroupToTrack() {
        
        PlayableDirector director = MaterialSwitchEditorTestUtility.CreateDirectorWithTimelineAsset(
            out TimelineAsset timelineAsset
        );  
        TimelineEditorUtility.SelectDirectorInTimelineWindow(director);
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(3);
        
        MaterialSwitchTrack track = timelineAsset.CreateTrack<MaterialSwitchTrack>(null, "TestTrack");

        SelectionGroup group = SelectionGroupManager.GetOrCreateInstance().CreateSelectionGroup("New Group", Color.green);
        director.SetGenericBinding(track, group);
        TimelineClip clip = TimelineEditorReflection.CreateClipOnTrack(typeof(MaterialSwitchClip), track, 0);            
        yield return YieldEditorUtility.WaitForFramesAndIncrementUndo(3);
    }
    
}

} //end namespace
