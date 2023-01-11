using System;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    /// <summary>
    /// A non-generic base class for overriding material property in MaterialSwitchClip
    /// </summary>
    [Serializable]
    public abstract class MaterialSwitchProperty
    {
        //[SerializeField] internal string displayName;
        [SerializeField] internal string propertyName;
        [SerializeField] internal int    propertyId;
        [SerializeField] internal bool   overrideBaseValue = false;

        /// <summary>
        /// Check if the property is overridden 
        /// </summary>
        /// <returns>true if overridden, false otherwise.</returns>
        public bool IsOverridden() => overrideBaseValue;
        
        /// <summary>
        /// The name of the property
        /// </summary>
        /// <returns>The property name</returns>
        public string GetPropertyName() => propertyName;
        
    }

    /// <summary>
    /// A generic base class for overriding material property in MaterialSwitchClip
    /// </summary>
    [Serializable]
    public abstract class MaterialProperty<T> : MaterialSwitchProperty
    {
        [SerializeField] internal T baseValue;
        [SerializeField] internal T targetValue;
        
        /// <summary>
        /// Get the value to be used for overriding a property.
        /// </summary>
        /// <returns>The value for overriding a property.</returns>
        public T GetTargetValue() => targetValue;
    }

    /// <summary>
    /// A class for overriding a float property in MaterialSwitchClip
    /// </summary>
    [System.Serializable]
    public class FloatProperty : MaterialProperty<float>
    {

    }

    /// <summary>
    /// A class for overriding a range property in MaterialSwitchClip
    /// </summary>
    [System.Serializable]
    public class RangeProperty : FloatProperty
    {
        [SerializeField] internal Vector2 rangeLimits;
    }

    /// <summary>
    /// A class for overriding a vector property in MaterialSwitchClip
    /// </summary>
    [System.Serializable]
    public class VectorProperty : MaterialProperty<Vector4>
    {

    }

    /// <summary>
    /// A class for overriding texture property in MaterialSwitchClip
    /// </summary>
    [System.Serializable]
    public class TextureProperty : MaterialProperty<Texture>
    {
        [SerializeField] internal RenderTexture finalTexture;
    }

    /// <summary>
    /// A class for overriding color property in MaterialSwitchClip
    /// </summary>
    [System.Serializable]
    public class ColorProperty : MaterialProperty<Color>
    {
        [SerializeField] internal Vector2 uv;
    }


}