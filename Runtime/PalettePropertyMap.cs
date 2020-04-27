using System.Collections.Generic;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    [System.Serializable]
    public class PalettePropertyMap
    {
        public Texture2D texture;
        public List<ColorProperty> colorCoordinates = new List<ColorProperty>();
        public Material material;
        public MaterialPropertyBlock materialPropertyBlock;
        public bool showCoords = false;
        public bool showTextures = false;
        public bool showFloats = false;
        public List<TextureProperty> textureProperties = new List<TextureProperty>();
        public List<FloatProperty> floatProperties = new List<FloatProperty>();
    }
}