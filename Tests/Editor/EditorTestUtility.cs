using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitch.EditorTests
{
internal static class EditorTestUtility
{

    
    //[TODO-sin:2021-10-25] Move to FIU
    internal static TimelineClip CreateTrackAndClip<T>(TimelineAsset timelineAsset, string trackName) 
        where T: TrackAsset, new() 
    {
        T            track = timelineAsset.CreateTrack<T>(null, trackName);
        TimelineClip clip  = track.CreateDefaultClip();
        return clip;
    }

    //[TODO-sin: 2021-9-10] Move to FIU ?
    internal static void SelectDirectorInTimelineWindow(PlayableDirector director) {
        //Select gameObject and open Timeline Window. This will trigger the TimelineWindow's update etc.
        EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
        Selection.activeObject = director;        
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    //[TODO-sin:2021-10-25] Move to FIU
    internal static void DestroyTimelineAssets(TimelineClip clip) {
        TrackAsset    movieTrack    = clip.GetParentTrack();
        TimelineAsset timelineAsset = movieTrack.timelineAsset;
            
        string tempTimelineAssetPath = AssetDatabase.GetAssetPath(timelineAsset);
        Assert.False(string.IsNullOrEmpty(tempTimelineAssetPath));

        timelineAsset.DeleteTrack(movieTrack);
        Destroy(timelineAsset, allowDestroyingAssets:true );
        AssetDatabase.DeleteAsset(tempTimelineAssetPath);
            
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    //[TODO-sin:2021-10-25] Move to FIU
    internal static void Destroy(Object obj, bool forceImmediate = false, bool allowDestroyingAssets = false) {
        if (!Application.isPlaying || forceImmediate) {
            Object.DestroyImmediate(obj,allowDestroyingAssets);                        
        } else {
            Object.Destroy(obj);            
        }
    }
    
    //[TODO-sin:2021-10-25] Move to FIU
    internal static IEnumerator WaitForFrames(int numFrames) {
        for (int i = 0; i < numFrames; ++i) {
            yield return null;
            
        }        
        Undo.IncrementCurrentGroup();
    }        

    
}

} //end namespace
