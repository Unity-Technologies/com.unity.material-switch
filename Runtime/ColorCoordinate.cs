using UnityEngine;

namespace Unity.MaterialSwitch
{
    // [System.Serializable]
    // public struct ColorCoordinate
    // {
    //     public Vector2 uv;
    //     public string propertyName;
    //     public string _propertyName;
    //     public int propertyId;
    //     public Color sampledColor;
    //     public Color originalColor;
    // }

    internal class MaterialProperty<T>
    {
        public string displayName;
        public string propertyName;
        public int propertyId;
        public bool overrideBaseValue = false;
        public T baseValue;
        public T targetValue;
    }

    [System.Serializable]
    internal class FloatProperty : MaterialProperty<float>
    {

    }

    [System.Serializable]
    internal class RangeProperty : FloatProperty
    {
        public Vector2 rangeLimits;
    }

    [System.Serializable]
    internal class VectorProperty : MaterialProperty<Vector4>
    {

    }

    [System.Serializable]
    internal class TextureProperty : MaterialProperty<Texture2D>
    {
        public RenderTexture finalTexture;
    }

    [System.Serializable]
    internal class ColorProperty : MaterialProperty<Color>
    {
        public Vector2 uv;
    }


}