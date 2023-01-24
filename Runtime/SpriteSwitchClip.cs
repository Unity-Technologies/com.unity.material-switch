using UnityEngine;
using UnityEngine.Playables;

namespace Unity.MaterialSwitch
{
    public class SpriteSwitchClip : PlayableAsset
    {
        public Sprite sprite;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SpriteSwitchPlayableBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.clip = this;
            return playable;
        }
    }
}