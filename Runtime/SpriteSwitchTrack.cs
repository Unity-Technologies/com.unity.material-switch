using System.Collections.Generic;
using Unity.FilmInternalUtilities;
using Unity.SelectionGroups;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitch
{
    /// <summary>
    /// Tracks for changing between sprites parameters. 
    /// </summary>
    [TrackClipType(typeof(SpriteSwitchClip))]
    [TrackBindingType(typeof(SelectionGroup))]
    internal class SpriteSwitchTrack : TrackAsset
    {
        
        public override Playable CreateTrackMixer(UnityEngine.Playables.PlayableGraph graph, UnityEngine.GameObject go, int inputCount)
        {
#if UNITY_EDITOR
            List<TimelineClip> clips        = new List<TimelineClip>(GetClips());
            AnalyticsSender.SendEventInEditor(new SpriteSwitchTrackMixerEvent(clips.Count));            
#endif                        
            return ScriptPlayable<SpriteSwitchMixerPlayableBehaviour>.Create(graph, inputCount);
        }
    }
}