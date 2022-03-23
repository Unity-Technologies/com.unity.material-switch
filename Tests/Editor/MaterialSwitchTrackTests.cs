using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitch.EditorTests
{
internal class MaterialSwitchTrackTests
{
//----------------------------------------------------------------------------------------------------------------------    

    [UnityTest]
    public IEnumerator CreateEmptyPlayableAsset() {
        TimelineAsset    timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
        PlayableDirector director      = new GameObject("Director").AddComponent<PlayableDirector>();  
        
        TimelineEditorUtility.CreateTrackAndClip<MaterialSwitchTrack, MaterialSwitchClip>(timelineAsset, "TestTrack");
        director.playableAsset = timelineAsset;
        TimelineEditorUtility.SelectDirectorInTimelineWindow(director);
        yield return EditorTestsUtility.WaitForFrames(3);

    }

//----------------------------------------------------------------------------------------------------------------------
    //[TODO-sin: 2021-11-10] Include this test as well
    [Ignore("CreateClip")]
    [UnityTest]
    public IEnumerator CreateClip() {
        TimelineAsset timelineAsset = TimelineEditorUtility.CreateAsset(MaterialSwitchTestEditorConstants.TEST_TIMELINE_ASSET_PATH);
        yield return EditorTestsUtility.WaitForFrames(3);
        
        TimelineClip clip = TimelineEditorUtility.CreateTrackAndClip(timelineAsset, "TestTrack",
            typeof(MaterialSwitchTrack), typeof(MaterialSwitchClip));
        
        yield return EditorTestsUtility.WaitForFrames(3);

        TimelineEditorUtility.DestroyAssets(clip);
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    [UnityTest]
    public IEnumerator AssignSelectionGroupToTrack() {
        TimelineAsset timelineAsset = TimelineEditorUtility.CreateAsset(MaterialSwitchTestEditorConstants.TEST_TIMELINE_ASSET_PATH);
        yield return EditorTestsUtility.WaitForFrames(3);
        
        PlayableDirector    director = new GameObject("Director").AddComponent<PlayableDirector>();
        MaterialSwitchTrack track    = timelineAsset.CreateTrack<MaterialSwitchTrack>(null, "TestTrack");
        director.playableAsset = timelineAsset;
        TimelineEditorUtility.SelectDirectorInTimelineWindow(director);
        yield return EditorTestsUtility.WaitForFrames(3);

        SelectionGroup group = SelectionGroupManager.GetOrCreateInstance().CreateSelectionGroup("New Group", Color.green);
        director.SetGenericBinding(track, group);
        TimelineClip clip = TimelineEditorReflection.CreateClipOnTrack(typeof(MaterialSwitchClip), track, 0);            
        yield return EditorTestsUtility.WaitForFrames(3);

        TimelineEditorUtility.DestroyAssets(clip);
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    
}

} //end namespace
