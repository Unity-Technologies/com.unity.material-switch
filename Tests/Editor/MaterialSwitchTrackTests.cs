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
        TimelineClip clip = EditorTestUtility.CreateTrackAndClip<MaterialSwitchTrack>(timelineAsset, "TrackWithEmptyDefaultClip");
        director.playableAsset = timelineAsset;
        yield return null;

        EditorTestUtility.DestroyTimelineAssets(clip);
    }

    
}

} //end namespace
