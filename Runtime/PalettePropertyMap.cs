using System.Collections.Generic;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    [System.Serializable]
    public class PalettePropertyMap
    {
        public Texture2D texture;
        public List<ColorCoordinate> colorCoordinates = new List<ColorCoordinate>();
        public Material material;
        public MaterialPropertyBlock materialPropertyBlock;
        public bool showCoords = false;
        public List<TextureProperty> textureProperties = new List<TextureProperty>();
    }
}