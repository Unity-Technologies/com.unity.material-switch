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

            //a group has many renderers.
            var renderers = group.GetComponent<SelectionGroups.Runtime.SelectionGroup>().GetMemberComponents<Renderer>();

            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                totalWeight += weight;
            }
            var missingWeight = 1f - totalWeight;

            //get materials from each renderer, then match to palettePropertyMap.

            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                if (weight == 0) continue;

                var paletteSwitchBehaviour = ((ScriptPlayable<PaletteSwitchBehaviour>)playable.GetInput(i)).GetBehaviour();
                //each renderer in the group can have many materials.
                foreach (var renderer in renderers)
                {
                    for (var index = 0; index < renderer.sharedMaterials.Length; index++)
                    {
                        var material = renderer.sharedMaterials[index];
                        //get the matching property map from this clip for a material.
                        var ppm = paletteSwitchBehaviour.GetMap(material);
                        if (ppm.materialPropertyBlock == null) ppm.materialPropertyBlock = new MaterialPropertyBlock();
                        renderer.SetPropertyBlock(ppm.materialPropertyBlock, index);
                        foreach (var cc in ppm.colorCoordinates)
                        {
                            var color = ppm.materialPropertyBlock.GetColor(cc.propertyId);
                            if(missingWeight > 0) {
                                color = Color.Lerp(color, cc.originalColor, missingWeight);
                            }
                            color = Color.Lerp(color, cc.sampledColor, weight);
                            ppm.materialPropertyBlock.SetColor(cc.propertyId, color);
                        }
                    }
                }
            }

        }

    }
}