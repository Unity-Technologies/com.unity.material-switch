using System;
using System.Collections.Generic;
using Unity.SelectionGroups;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace Unity.MaterialSwitch
{

    /// <summary>
    /// A Timeline clip for changing and blending between material parameters.
    /// </summary>
    public class MaterialSwitchClip : PlayableAsset
    {
        
        [FormerlySerializedAs("globalPalettePropertyMap")] [SerializeField] internal MaterialProperties globalMaterialProperties;
        [FormerlySerializedAs("palettePropertyMap")] [SerializeField] internal List<MaterialProperties> materialPropertiesList;

        /// <summary>
        /// Enumerate all material properties set in the clip
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MaterialProperties> GetMaterialProperties() 
        {
            foreach (MaterialProperties mp in materialPropertiesList) 
                yield return mp;
        }

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