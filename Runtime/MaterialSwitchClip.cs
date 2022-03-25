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

        /// <summary>
        /// Override a texture property in a certain material.
        /// </summary>
        /// <param name="mat">The material which has the property.</param>
        /// <param name="propertyName">The name of the property to be overridden</param>
        /// <param name="tex">The texture for overriding.</param>
        /// <returns>True if the applicable property is found and overridable, false otherwise.</returns>
        public bool OverrideTextureProperty(Material mat, string propertyName, Texture2D tex) 
        {
            MaterialProperties mp = FindMaterialProperties(mat);
            TextureProperty p = mp?.FindTextureProperty(propertyName);
            if (p == null)
                return false;

            p.targetValue       = tex;
            p.overrideBaseValue = true;
            return true;
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

        [CanBeNull]
        MaterialProperties FindMaterialProperties(Material mat) 
        {
            foreach (MaterialProperties mp in materialPropertiesList) 
            {
                if (mp.material == mat)
                    return mp;
            }

            return null;
        }
        
    }
}