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
    }
}