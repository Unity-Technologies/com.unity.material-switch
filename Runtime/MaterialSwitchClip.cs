using System.Collections.Generic;
using JetBrains.Annotations;
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
        /// Override a property in a certain material.
        /// </summary>
        /// <param name="mat">The material which has the property.</param>
        /// <param name="propertyName">The name of the property to be overridden</param>
        /// <param name="obj">The object for overriding.</param>
        /// <returns>True if the applicable property is found and overridable, false otherwise.</returns>
        public bool OverrideProperty<T>(Material mat, string propertyName, T obj) 
        {
            MaterialProperties  mp = FindMaterialProperties(mat);
            if (null == mp)
                return false;

            MaterialProperty<T> p = null;
            if (typeof(Texture2D) == typeof(T) ) {
                 p = mp?.FindTextureProperty(propertyName) as MaterialProperty<T>;
            } else if (typeof(float) == typeof(T) ) { 
                p = mp?.FindFloatProperty(propertyName) as MaterialProperty<T>;
            } else if (typeof(Color) == typeof(T) ) { 
                p = mp?.FindColorProperty(propertyName) as MaterialProperty<T>;
            }
            if (null==p)
                return false;

            p.targetValue       = obj;
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