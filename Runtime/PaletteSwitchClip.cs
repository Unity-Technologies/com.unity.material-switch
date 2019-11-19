using System.Collections.Generic;
using Unity.SelectionGroups;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.PaletteSwitch
{
    // [CreateAssetMenu]
    public class PaletteSwitchClip : PlayableAsset
    {
        public PaletteAsset paletteAsset;
        [Space]
        public PropertyChangeCollection propertyOverrides;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<PaletteSwitchBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.paletteAsset = paletteAsset;
            behaviour.propertyOverrides = propertyOverrides;
            return playable;
        }
    }
}