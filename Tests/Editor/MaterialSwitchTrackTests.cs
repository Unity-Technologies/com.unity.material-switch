using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using Unity.SelectionGroups;
using Unity.SelectionGroups.Runtime;
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
        TimelineAsset timelineAsset = TimelineEditorUtility.CreateAsset(MaterialSwitchTestEditorConstants.TEST_TIMELINE_ASSET_PATH);
        
        PlayableDirector  director = new GameObject("Director").AddComponent<PlayableDirector>();  
        TimelineClip clip = TimelineEditorUtility.CreateTrackAndClip<MaterialSwitchTrack, MaterialSwitchClip>(
            timelineAsset, "TrackWithEmptyDefaultClip");
        director.playableAsset = timelineAsset;
        TimelineEditorUtility.SelectDirectorInTimelineWindow(director);
        yield return EditorTestsUtility.WaitForFrames(3);

        TimelineEditorUtility.DestroyAssets(clip);
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

        SelectionGroup group = CreateSceneSelectionGroup("New Group", string.Empty, Color.green, new List<Object>());
        director.SetGenericBinding(track, group);
        TimelineClip clip = TimelineEditorReflection.CreateClipOnTrack(typeof(MaterialSwitchClip), track, 0);            
        yield return EditorTestsUtility.WaitForFrames(3);

        TimelineEditorUtility.DestroyAssets(clip);
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    

    //[TODO-sin: 2021-10-25] make CreateSceneSelectionGroup() into a public API 
    SelectionGroup CreateSceneSelectionGroup(string name, string query, Color color, IList<Object> members)
    {
        var g = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(g,"New Scene Selection Group");
        var group = g.AddComponent<Unity.SelectionGroups.Runtime.SelectionGroup>();
        group.Name        = name;
        group.Query       = query;
        group.Color       = color;
        group.Scope       = SelectionGroupDataLocation.Scene;
        group.ShowMembers = true;
        group.Add(members);
        SelectionGroupManager.Register(group);
        return group;
    }
    
    
}

} //end namespace
