using Unity.SelectionGroups;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.PaletteSwitch
{
    [TrackClipType(typeof(PaletteSwitchClip))]
    [TrackBindingType(typeof(SelectionGroup))]
    public class PaletteSwitchTrack : TrackAsset
    {
        // public override Playable CreateTrackMixer(UnityEngine.Playables.PlayableGraph graph, UnityEngine.GameObject go, int inputCount)
        // {
        //     return ScriptPlayable<PaletteSwitchBehaviour>.Create(graph, inputCount);
        // }


    }
}