using UnityEngine;

namespace Unity.MaterialSwitch
{
    [System.Serializable]
    public struct ColorCoordinate
    {
        public Vector2 uv;
        public string propertyName;
        public int propertyId;
        public Color sampledColor;
        public Color originalColor;
    }
}