using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace Unity.MaterialSwitch
{
    internal class SpriteSwitchClip : PlayableAsset
    {
        public Texture2D spriteSheet;
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SpriteSwitchPlayableBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.clip = this;
            return playable;
        }
    }
}