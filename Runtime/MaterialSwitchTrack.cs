using System;
using Unity.SelectionGroups;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitch
{
    /// <summary>
    /// Tracks for changing and blending between material parameters. 
    /// </summary>
    [TrackClipType(typeof(MaterialSwitchClip))]
    [TrackBindingType(typeof(SelectionGroup))]
    public class MaterialSwitchTrack : TrackAsset
    {
        
        public override Playable CreateTrackMixer(UnityEngine.Playables.PlayableGraph graph, UnityEngine.GameObject go, int inputCount)
        {
            var director = go.GetComponent<PlayableDirector>();
            return ScriptPlayable<MaterialSwitchMixerPlayableBehaviour>.Create(graph, inputCount);
        }
    }
}