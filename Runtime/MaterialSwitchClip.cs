using System;
using System.Collections.Generic;
using Unity.SelectionGroups.Runtime;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace Unity.MaterialSwitch
{

    internal class MaterialSwitchClip : PlayableAsset
    {
        
        [FormerlySerializedAs("globalPalettePropertyMap")] public MaterialProperties globalMaterialProperties;
        [FormerlySerializedAs("materialPropertiesList")] public List<MaterialProperties> materialPropertiesList;

        void OnValidate()
        {
            foreach (var ppm in materialPropertiesList)
            {
                if (ppm.texture == null) continue;
                if(!ppm.texture.isReadable) continue;
                for (int i = 0; i < ppm.colorProperties.Count; i++)
                {
                    var cc = ppm.colorProperties[i];
                    cc.targetValue = ppm.texture.GetPixel((int)cc.uv.x, (int)cc.uv.y);
                    ppm.colorProperties[i] = cc;
                }
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<MaterialSwitchPlayableBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.clip = this;
            behaviour.materialPropertiesList = materialPropertiesList;
            return playable;
        }
    }
}