using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.PaletteSwitch
{
    public class PaletteSwitchMixerBehaviour : PlayableBehaviour
    {
        Dictionary<string, PropertyChange> propertyMap = new Dictionary<string, PropertyChange>();

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var group = playerData as SelectionGroups.SelectionGroup;
            if (group == null) return;
            propertyMap.Clear();

            var inputCount = playable.GetInputCount();

            var totalWeight = 0f;

            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                if (weight == 0) continue;
                totalWeight += weight;

                var paletteSwitchBehaviour = ((ScriptPlayable<PaletteSwitchBehaviour>)playable.GetInput(i)).GetBehaviour();
                var changes = paletteSwitchBehaviour.propertyOverrides.items;
                if (paletteSwitchBehaviour.paletteAsset != null)
                {
                    if (paletteSwitchBehaviour.paletteAsset.propertyChanges.items != null)
                        changes = changes.Concat(paletteSwitchBehaviour.paletteAsset.propertyChanges.items).ToArray();
                }

                foreach (var cc in changes)
                {
                    if (!propertyMap.TryGetValue(cc.UID, out PropertyChange existing))
                    {
                        propertyMap[cc.UID] = cc;
                    }
                    else
                    {
                        //move existing values towards target values using weight.
                        existing.colorValue = Color.Lerp(existing.colorValue, cc.colorValue, weight);
                        existing.floatValue = Mathf.Lerp(existing.floatValue, cc.floatValue, weight);
                        existing.vectorValue = Vector4.Lerp(existing.vectorValue, cc.vectorValue, weight);
                        propertyMap[cc.UID] = existing;
                    }
                }
            }

            var finalValues = propertyMap.Values.ToArray();
            //do we need to mix in base material value?
            if (totalWeight < 1)
            {
                for (var i = 0; i < finalValues.Length; i++)
                {
                    var cc = finalValues[i];
                    switch (cc.propertyType)
                    {
                        case PropertyChange.COLOR:
                            if (PropertyChangeCollection.GetDefaultColor(group, cc, out Color color))
                                cc.colorValue = Color.Lerp(color, cc.colorValue, totalWeight);
                            break;
                        case PropertyChange.FLOAT:
                            if (PropertyChangeCollection.GetDefaultFloat(group, cc, out float value))
                                cc.floatValue = Mathf.Lerp(value, cc.floatValue, totalWeight);
                            break;
                        case PropertyChange.VECTOR:
                            if (PropertyChangeCollection.GetDefaultVector(group, cc, out Vector4 vector))
                                cc.vectorValue = Vector4.Lerp(vector, cc.vectorValue, totalWeight);
                            break;
                    }
                    finalValues[i] = cc;
                }
            }
            // foreach (var cc in colorMap.Values) Debug.Log(cc.color);
            PropertyChangeCollection.SetPropertyBlock(group, finalValues);
        }


    }
}