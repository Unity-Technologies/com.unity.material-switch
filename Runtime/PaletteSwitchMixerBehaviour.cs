using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.PaletteSwitch
{
    public class PaletteSwitchMixerBehaviour : PlayableBehaviour
    {
        Dictionary<string, ColorChange> colorMap = new Dictionary<string, ColorChange>();

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var group = playerData as SelectionGroups.SelectionGroup;
            if (group == null) return;
            colorMap.Clear();

            var inputCount = playable.GetInputCount();

            var totalWeight = 0f;

            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                if (weight == 0) continue;
                totalWeight += weight;

                var paletteSwitchBehaviour = ((ScriptPlayable<PaletteSwitchBehaviour>)playable.GetInput(i)).GetBehaviour();
                if (paletteSwitchBehaviour.paletteAsset == null) continue;
                var palette = paletteSwitchBehaviour.paletteAsset.palette;

                foreach (var cc in palette)
                {
                    if (!colorMap.TryGetValue(cc.UID, out ColorChange existing))
                    {
                        colorMap[cc.UID] = cc;
                    }
                    else
                    {
                        //move existing color towards target color using weight.
                        existing.color = Color.Lerp(existing.color, cc.color, weight);
                        colorMap[cc.UID] = existing;
                    }
                }
            }

            var finalColors = colorMap.Values.ToArray();
            //do we need to mix in base material color?
            if (totalWeight < 1)
            {
                for (var i = 0; i < finalColors.Length; i++)
                {
                    var cc = finalColors[i];
                    if (PaletteAsset.GetDefaultColor(group, cc, out Color color))
                    {
                        cc.color = Color.Lerp(color, cc.color, totalWeight);
                        finalColors[i] = cc;
                    }
                }
            }
            // foreach (var cc in colorMap.Values) Debug.Log(cc.color);
            PaletteAsset.SetPropertyBlock(group, finalColors);
        }


    }
}