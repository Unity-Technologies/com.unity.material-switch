using System.Collections.Generic;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    internal class RemapNameCache
    {
        Dictionary<Shader,MaterialPropertyNameMap> _nameRemaps = null;
        
        internal (string displayName, bool hidden) GetDisplayName(Material material, string propertyName)
        {
            if(_nameRemaps == null) CollectRemaps();
            
            var displayName = propertyName;
            var isHidden = false;
            
            if (material != null && material.shader != null)
            {
                if (_nameRemaps.TryGetValue(material.shader, out var nameMap))
                {
                    if (nameMap.TryGetValue(propertyName, out var remappedName))
                    {
                        displayName = remappedName.displayName;
                        isHidden = remappedName.hidden;
                    }
                }
            }
            return (displayName, isHidden);
        }
        
        void CollectRemaps()
        {
            var mapAssets = Resources.FindObjectsOfTypeAll<MaterialPropertyNameMap>();
            _nameRemaps ??= new Dictionary<Shader, MaterialPropertyNameMap>();
            _nameRemaps.Clear();
            foreach (var i in mapAssets)
            {
                if (i != null && i.shader != null)
                {
                    _nameRemaps[i.shader] = i;
                }
            }
        }
    }
}