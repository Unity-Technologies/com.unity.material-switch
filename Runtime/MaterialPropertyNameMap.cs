using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.MaterialSwitch
{
    [CreateAssetMenu]
    public class MaterialPropertyNameMap : ScriptableObject, ISerializationCallbackReceiver
    {
        public Material material;

        public Shader Shader => material?.shader;
        
        [System.Serializable]
        public struct PropertyDisplayName
        {
            [HideInInspector]
            public string propertyName;
            public string displayName;
        }

        public List<PropertyDisplayName> nameMap = new List<PropertyDisplayName>();
        private Dictionary<string, string> nameMapIndex = new Dictionary<string, string>();

        public string this[string key] => nameMapIndex[key];
        
        public bool TryGetValue(string propertyName, out string displayName) => nameMapIndex.TryGetValue(propertyName, out displayName);

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            foreach (var i in nameMap)
            {
                nameMapIndex[i.propertyName] = i.displayName;
            }            
        }
        
    }
}