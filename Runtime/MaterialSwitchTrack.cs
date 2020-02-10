using Unity.SelectionGroups.Runtime;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitch
{
    [TrackClipType(typeof(MaterialSwitchClip))]
    [TrackBindingType(typeof(MaterialGroup))]
    public class MaterialSwitchTrack : TrackAsset
    {
        UnityEngine.Playables.PlayableGraph graph;

        public override Playable CreateTrackMixer(UnityEngine.Playables.PlayableGraph graph, UnityEngine.GameObject go, int inputCount)
        {
            var director = go.GetComponent<PlayableDirector>();
            return ScriptPlayable<MaterialSwitchMixerPlayableBehaviour>.Create(graph, inputCount);
        }

    }
}