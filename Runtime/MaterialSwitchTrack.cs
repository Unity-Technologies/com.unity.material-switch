using System.Collections.Generic;
using Unity.FilmInternalUtilities;
using Unity.SelectionGroups;
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

#if UNITY_EDITOR
            List<TimelineClip> clips = new List<TimelineClip>(GetClips());
            int numMaterials = 0;
            
            PlayableDirector director = go.GetComponent<PlayableDirector>();
            SelectionGroup   sg       = director.GetGenericBinding(this) as SelectionGroup;
            if (null != sg) {
                MaterialGroup mg = sg.gameObject.GetComponent<MaterialGroup>();
                if (null != mg) {
                    numMaterials = mg.sharedMaterials.Length;
                }
            }
            AnalyticsSender.SendEventInEditor(new MaterialSwitchTrackMixerEvent(clips.Count, numMaterials));
            
#endif            
            
            return ScriptPlayable<MaterialSwitchMixerPlayableBehaviour>.Create(graph, inputCount);
        }
    }
}