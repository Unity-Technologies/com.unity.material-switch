using UnityEngine;

namespace Unity.PaletteSwitch
{
    [System.Serializable]
    public struct ColorChange
    {
        public bool enabled;
        public string memberNameQuery;
        public int materialIndex;
        public string propertyName;
        public string propertyDisplayName;
        public Color color;

        public string UID => $"{memberNameQuery}.{materialIndex}.{propertyName}";
    }
}