using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
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
    [UnitySetUp]
    public IEnumerator Setup() {
        new GameObject("Director").AddComponent<PlayableDirector>();       
        yield return null;
    }

//----------------------------------------------------------------------------------------------------------------------    


    [Test]
    public void CreateEmptyPlayableAsset() {
        PlayableDirector  director = new GameObject("Director").AddComponent<PlayableDirector>();  
        TimelineAsset timelineAsset = TimelineEditorUtility.CreateAsset(MaterialSwitchTestEditorConstants.TEST_TIMELINE_ASSET_PATH);
        TimelineClip clip = CreateTrackAndClip<MaterialSwitchTrack>(timelineAsset, "TrackWithEmptyDefaultClip");
        director.playableAsset = timelineAsset;
        DestroyTestTimelineAssets(clip);
    }

//----------------------------------------------------------------------------------------------------------------------    
    
    //[TODO-sin:2021-10-25] Move to FIU
    internal static TimelineClip CreateTrackAndClip<T>(TimelineAsset timelineAsset, string trackName) 
        where T: TrackAsset, new() 
    {
        T            track = timelineAsset.CreateTrack<T>(null, trackName);
        TimelineClip clip  = track.CreateDefaultClip();
        return clip;
    }
    
//----------------------------------------------------------------------------------------------------------------------                
    //[TODO-sin:2021-10-25] Move to FIU
    internal static void DestroyTestTimelineAssets(TimelineClip clip) {
        TrackAsset    movieTrack    = clip.GetParentTrack();
        TimelineAsset timelineAsset = movieTrack.timelineAsset;
            
        string tempTimelineAssetPath = AssetDatabase.GetAssetPath(timelineAsset);
        Assert.False(string.IsNullOrEmpty(tempTimelineAssetPath));

        timelineAsset.DeleteTrack(movieTrack);
        ObjectUtility.Destroy(timelineAsset);
        AssetDatabase.DeleteAsset(tempTimelineAssetPath);
            
    }
    
    
}

} //end namespace
