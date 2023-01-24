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
    public class SpriteSwitchTrack : TrackAsset
    {
        
        public override Playable CreateTrackMixer(UnityEngine.Playables.PlayableGraph graph, UnityEngine.GameObject go, int inputCount)
        {
            var director = go.GetComponent<PlayableDirector>();
            return ScriptPlayable<SpriteSwitchMixerPlayableBehaviour>.Create(graph, inputCount);
        }
    }
}