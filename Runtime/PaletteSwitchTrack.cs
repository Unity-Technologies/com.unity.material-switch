using Unity.SelectionGroups.Runtime;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.PaletteSwitch
{
    [TrackClipType(typeof(PaletteSwitchClip))]
    [TrackBindingType(typeof(MaterialPropertyGroup))]
    public class PaletteSwitchTrack : TrackAsset
    {
        UnityEngine.Playables.PlayableGraph graph;

        public override Playable CreateTrackMixer(UnityEngine.Playables.PlayableGraph graph, UnityEngine.GameObject go, int inputCount)
        {
            var director = go.GetComponent<PlayableDirector>();
            return ScriptPlayable<PaletteSwitchMixerBehaviour>.Create(graph, inputCount);
        }

    }
}