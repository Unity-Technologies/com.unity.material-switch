using System;
using System.Collections.Generic;
using Unity.SelectionGroups;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.PaletteSwitch
{

    public class PaletteSwitchClip : PlayableAsset
    {
        public Material[] targetMaterials;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<PaletteSwitchBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.targetMaterials = targetMaterials;
            return playable;
        }
    }
}