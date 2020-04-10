using UnityEngine;

namespace Unity.MaterialSwitch
{
    [System.Serializable]
    public struct ColorCoordinate
    {
        public Vector2 uv;
        public string propertyName;
        public string _propertyName;
        public int propertyId;
        public Color sampledColor;
        public Color originalColor;
    }

    public class MaterialProperty<T>
    {
        public string propertyName;
        public int propertyId;
        public T originalValue;
        public T targetValue;
    }

    [System.Serializable]
    public class FloatProperty : MaterialProperty<float>
    {

    }

    [System.Serializable]
    public class VectorProperty : MaterialProperty<Vector4>
    {

    }

    [System.Serializable]
    public class TextureProperty : MaterialProperty<Texture2D>
    {
        public RenderTexture finalTexture;
    }

    [System.Serializable]
    public class ColorProperty : MaterialProperty<Color>
    {

    }


}