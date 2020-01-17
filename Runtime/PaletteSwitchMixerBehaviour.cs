using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.PaletteSwitch
{
    public class PaletteSwitchMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var group = playerData as MaterialPropertyGroup;
            if (group == null) return;

            var inputCount = playable.GetInputCount();

            var totalWeight = 0f;
            group.RestoreOriginalMaterials();
            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                if (weight == 0) continue;
                totalWeight += weight;

                var paletteSwitchBehaviour = ((ScriptPlayable<PaletteSwitchBehaviour>)playable.GetInput(i)).GetBehaviour();
                
                group.LerpTowards(paletteSwitchBehaviour.targetMaterials, weight);
                
            }


            //if total weight is less than one, we need to mix in original material values.
            if (totalWeight < 1)
            {
               group.LerpTowards(group.originalMaterials, 1f-totalWeight);
            }
            
            //set material property block on group
        }

    }
}