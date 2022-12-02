using System.Collections.Generic;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    internal class RemapNameCache
    {
        List<MaterialPropertyNameMap> _nameRemaps = null;
        
        internal (string displayName, bool hidden) GetDisplayName(Material material, string propertyName)
        {
            if(_nameRemaps == null) CollectRemaps();
            
            var displayName = propertyName;
            var isHidden = false;
            
            if (material != null && material.shader != null)
            {
                foreach(var map in _nameRemaps)
                {
                    if(map.shader == material.shader)
                    {
                        if (map.TryGetValue(propertyName, out var remappedName))
                        {
                            displayName = remappedName.displayName;
                            isHidden = remappedName.hidden;
                            break;
                        }
                    }
                }
            }
            return (displayName, isHidden);
        }
        
        void CollectRemaps()
        {
            var mapAssets = Resources.FindObjectsOfTypeAll<MaterialPropertyNameMap>();
            _nameRemaps ??= new List<MaterialPropertyNameMap>();
            _nameRemaps.Clear();
            foreach (var i in mapAssets)
            {
                if (i != null)
                {
                    _nameRemaps.Add(i);
                }
            }
        }
    }
}