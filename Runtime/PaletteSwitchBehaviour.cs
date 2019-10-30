using UnityEngine;
using UnityEngine.Playables;

namespace Unity.PaletteSwitch
{
    public class PaletteSwitchBehaviour : PlayableBehaviour
    {
        public PaletteAsset paletteAsset;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var group = playerData as SelectionGroups.SelectionGroup;
            if (group == null) return;
            if (paletteAsset == null)
            {
                PaletteAsset.ClearPropertyBlock(group);
            }
            else
            {
                paletteAsset.SetPropertyBlock(group, info.weight);
            }
        }
    }
}