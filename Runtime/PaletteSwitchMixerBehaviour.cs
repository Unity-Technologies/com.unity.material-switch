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

            //a group has many renderers.
            var renderers = group.GetComponent<SelectionGroups.Runtime.SelectionGroup>().GetMemberComponents<Renderer>();

            var inputCount = playable.GetInputCount();
            var totalWeight = 0f;
            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                totalWeight += weight;
            }
            var missingWeight = 1f - totalWeight;

            if (missingWeight >= 1f)
            {
                RemoveMaterialPropertyBlocks(renderers);
                return;
            } 

            AssignMaterialPropertyBlocks(group, renderers);

            //get materials from each renderer, then match to palettePropertyMap.
            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                if (weight == 0) continue;

                var paletteSwitchBehaviour = ((ScriptPlayable<PaletteSwitchBehaviour>)playable.GetInput(i)).GetBehaviour();

                //each renderer in the group can have many materials.
                //calculate the colors coming from this clip and update relevant property block
                foreach (var renderer in renderers)
                {
                    for (var index = 0; index < renderer.sharedMaterials.Length; index++)
                    {
                        var material = renderer.sharedMaterials[index];

                        //get the matching property map from this clip for a material.
                        //the property map contains colors that added into the property block.
                        var ppm = paletteSwitchBehaviour.GetMap(material);

                        var mpb = group.GetMaterialPropertyBlock(material);
                        if(mpb.isEmpty) {
                            foreach (var cc in ppm.colorCoordinates) {
                                mpb.SetColor(cc.propertyId, cc.originalColor);
                            }
                        }

                        foreach (var cc in ppm.colorCoordinates)
                        {
                            var color = mpb.GetColor(cc.propertyId);
                            color = Color.Lerp(color, cc.sampledColor, weight);
                            mpb.SetColor(cc.propertyId, color);
                        }
                        renderer.SetPropertyBlock(mpb, index);
                    }
                }
            }

        }

        private static void AssignMaterialPropertyBlocks(MaterialPropertyGroup group, IEnumerable<Renderer> renderers)
        {
            foreach (var r in renderers)
            {
                for (var i = 0; i < r.sharedMaterials.Length; i++)
                {
                    var mpb = group.GetMaterialPropertyBlock(r.sharedMaterials[i]);
                    mpb.Clear();
                    r.SetPropertyBlock(mpb, i);
                }
            }
        }

        private static void RemoveMaterialPropertyBlocks(IEnumerable<Renderer> renderers)
        {
            foreach (var r in renderers)
            {
                for (var i = 0; i < r.sharedMaterials.Length; i++)
                {
                    r.SetPropertyBlock(null, i);
                }
            }
            return;
        }
    }
}