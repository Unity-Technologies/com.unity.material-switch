using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.MaterialSwitch
{
    [CreateAssetMenu]
    public class MaterialPropertyNameMap : ScriptableObject, ISerializationCallbackReceiver
    {
        public Shader shader;
        
        [System.Serializable]
        public struct PropertyDisplayName
        {
            [HideInInspector]
            public string propertyName;
            public bool hidden;
            public string displayName;
        }

        public List<PropertyDisplayName> nameMap = new List<PropertyDisplayName>();
        private Dictionary<string, PropertyDisplayName> nameMapIndex = new Dictionary<string, PropertyDisplayName>();

        public PropertyDisplayName this[string key] => nameMapIndex[key];
        
        public bool TryGetValue(string propertyName, out PropertyDisplayName displayName) => nameMapIndex.TryGetValue(propertyName, out displayName);

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            foreach (var i in nameMap)
            {
                nameMapIndex[i.propertyName] = i;
            }            
        }
        
    }
}