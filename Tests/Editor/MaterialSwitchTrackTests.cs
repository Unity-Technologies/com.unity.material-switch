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
//----------------------------------------------------------------------------------------------------------------------    


    [UnityTest]
    public IEnumerator CreateEmptyPlayableAsset() {
        TimelineAsset timelineAsset = TimelineEditorUtility.CreateAsset(MaterialSwitchTestEditorConstants.TEST_TIMELINE_ASSET_PATH);
        
        PlayableDirector  director = new GameObject("Director").AddComponent<PlayableDirector>();  
        TimelineClip clip = CreateTrackAndClip<MaterialSwitchTrack>(timelineAsset, "TrackWithEmptyDefaultClip");
        director.playableAsset = timelineAsset;
        yield return null;

        EditorTestUtility.DestroyTimelineAssets(clip);
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
    
    
    
}

} //end namespace
