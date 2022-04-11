using Unity.FilmInternalUtilities.Editor;
using Unity.SelectionGroups;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Assert = UnityEngine.Assertions.Assert;

namespace Unity.MaterialSwitch.EditorTests
{
internal static class MaterialSwitchEditorTestUtility
{
    internal static PlayableDirector CreateDirectorWithTimelineAsset(out TimelineAsset timelineAsset) {
        timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
        PlayableDirector director = new GameObject("Director").AddComponent<PlayableDirector>();
        Assert.IsNotNull(timelineAsset);

        director.playableAsset = timelineAsset;
        return director;
    }
    

    internal static PlayableDirector CreateDefaultDirectorAndTrack(out TimelineAsset timelineAsset, 
        out MaterialSwitchTrack track) 
    {        
        PlayableDirector director = MaterialSwitchEditorTestUtility.CreateDirectorWithTimelineAsset(
            out timelineAsset
        );  
        TimelineEditorUtility.SelectDirectorInTimelineWindow(director);        
        track = timelineAsset.CreateTrack<MaterialSwitchTrack>(null, "TestTrack");

        SelectionGroup group = SelectionGroupManager.GetOrCreateInstance().CreateSelectionGroup("New Group", Color.green);
        director.SetGenericBinding(track, group);
        return director;
    }
    
    
}

} //end namespace