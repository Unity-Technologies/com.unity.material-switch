using UnityEngine;
using UnityEngine.Playables;

namespace Unity.PaletteSwitch
{
    public class PaletteSwitchMixerBehaviour : PlayableBehaviour
    {

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var group = playerData as SelectionGroups.SelectionGroup;
            if (!group) return;
            var inputCount = playable.GetInputCount();

            var p = ScriptableObject.CreateInstance<PaletteAsset>();
            for (var i = 0; i < inputCount; i++)
            {
                var paletteSwitchBehaviour = ((ScriptPlayable<PaletteSwitchBehaviour>)playable.GetInput(i)).GetBehaviour();
                var weight = playable.GetInputWeight(i);
            }
        }

    }
}