using UnityEngine;

namespace Unity.PaletteSwitch
{
    [System.Serializable]
    public struct PropertyChange
    {
        public const int FLOAT = 0, COLOR = 1, VECTOR = 2;

        public bool enabled;
        public string memberNameQuery;
        public int materialIndex;
        public string propertyName;
        public string propertyDisplayName;
        public string UID => $"{memberNameQuery}.{materialIndex}.{propertyName}";

        public int propertyType;
        public Color colorValue;
        public float floatValue;
        public Vector4 vectorValue;
    }
}