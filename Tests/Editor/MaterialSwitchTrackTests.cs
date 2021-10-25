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
        director.playableAsset = timelineAsset;
        EditorTestUtility.SelectDirectorInTimelineWindow(director);        
        yield return null;

        TimelineClip clip = EditorTestUtility.CreateTrackAndClip<MaterialSwitchTrack>(timelineAsset, "TrackWithEmptyDefaultClip");
        yield return null;
        Assert.IsNotNull(clip.asset as MaterialSwitchClip);
        
        
        EditorTestUtility.DestroyTimelineAssets(clip);
    }

//----------------------------------------------------------------------------------------------------------------------    
    [UnityTest]
    public IEnumerator AssignSelectionGroupToTrack() {
        TimelineAsset timelineAsset = TimelineEditorUtility.CreateAsset(MaterialSwitchTestEditorConstants.TEST_TIMELINE_ASSET_PATH);
        yield return EditorTestUtility.WaitForFrames(3);
        
        PlayableDirector    director = new GameObject("Director").AddComponent<PlayableDirector>();
        MaterialSwitchTrack track    = timelineAsset.CreateTrack<MaterialSwitchTrack>(null, "TestTrack");
        director.playableAsset = timelineAsset;
        yield return EditorTestUtility.WaitForFrames(3);

        SelectionGroup group = CreateSceneSelectionGroup("New Group", string.Empty, Color.green, new List<Object>());
        director.SetGenericBinding(track, group);
        yield return EditorTestUtility.WaitForFrames(3);

        Selection.activeObject = timelineAsset;        
        Debug.Log(Selection.activeObject);
        
//        EditorTestUtility.SelectDirectorInTimelineWindow(director);        
        yield return EditorTestUtility.WaitForFrames(3);

        
        TimelineClip clip = track.CreateDefaultClip();
        yield return EditorTestUtility.WaitForFrames(3);

//        EditorTestUtility.DestroyTimelineAssets(clip);
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
