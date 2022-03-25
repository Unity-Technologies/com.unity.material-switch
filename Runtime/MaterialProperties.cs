using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.MaterialSwitch
{
    /// <summary>
    /// A class to store material properties 
    /// </summary>
    [System.Serializable]
    public class MaterialProperties
    {
        [SerializeField] internal Texture2D texture;
        [FormerlySerializedAs("colorCoordinates")] [SerializeField] internal List<ColorProperty> colorProperties = new List<ColorProperty>();
        [SerializeField] internal Material material;
        [SerializeField] internal bool showCoords = false;
        [SerializeField] internal bool showTextures = false;
        [SerializeField] internal bool showFloats = false;
        [SerializeField] internal List<TextureProperty> textureProperties = new List<TextureProperty>();
        [SerializeField] internal List<FloatProperty> floatProperties = new List<FloatProperty>();
        [SerializeField] internal bool needsUpdate = true;

        /// <summary>
        /// Enumerate all texture properties
        /// </summary>
        /// <returns>A texture property</returns>
        public IEnumerable<TextureProperty> GetTextureProperties() 
        {
            foreach (TextureProperty t in textureProperties)
                yield return t;
        }

        /// <summary>
        /// Enumerate all float properties
        /// </summary>
        /// <returns>A float property</returns>
        public IEnumerable<FloatProperty> GetFloatProperties() 
        {
            foreach (FloatProperty f in floatProperties)
                yield return f;
        }
        

        /// <summary>
        /// Enumerate all color properties
        /// </summary>
        /// <returns>A color property</returns>
        public IEnumerable<ColorProperty> GetColorProperties() 
        {
            foreach (ColorProperty c in colorProperties)
                yield return c;
        }

//----------------------------------------------------------------------------------------------------------------------        
        internal TextureProperty FindTextureProperty(string propertyName) 
        {
            return FindProperty(textureProperties, propertyName);
        }

        internal ColorProperty FindColorProperty(string propertyName) 
        {
            return FindProperty(colorProperties, propertyName);
        }

        internal FloatProperty FindFloatProperty(string propertyName) 
        {
            return FindProperty(floatProperties, propertyName);
        }

        private static T FindProperty<T>(IEnumerable<T> properties,string propertyName) where T:MaterialSwitchProperty 
        {
            foreach(T p in properties) 
            {
                if (p.propertyName == propertyName)
                    return p;
            }
            return null;
        }
        
    }
}