using System;
using Unity.SelectionGroups;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitch
{
    [TrackClipType(typeof(MaterialSwitchClip))]
    [TrackBindingType(typeof(SelectionGroup))]
    internal class MaterialSwitchTrack : TrackAsset
    {
        
        public override Playable CreateTrackMixer(UnityEngine.Playables.PlayableGraph graph, UnityEngine.GameObject go, int inputCount)
        {
            var director = go.GetComponent<PlayableDirector>();
            return ScriptPlayable<MaterialSwitchMixerPlayableBehaviour>.Create(graph, inputCount);
        }
    }
}