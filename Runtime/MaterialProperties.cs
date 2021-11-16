using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.MaterialSwitch
{
    [System.Serializable]
    internal class MaterialProperties
    {
        public Texture2D texture;
        [FormerlySerializedAs("colorCoordinates")] public List<ColorProperty> colorProperties = new List<ColorProperty>();
        public Material material;
        public MaterialPropertyBlock materialPropertyBlock;
        public bool showCoords = false;
        public bool showTextures = false;
        public bool showFloats = false;
        public List<TextureProperty> textureProperties = new List<TextureProperty>();
        public List<FloatProperty> floatProperties = new List<FloatProperty>();
        public bool needsUpdate = true;
    }
}