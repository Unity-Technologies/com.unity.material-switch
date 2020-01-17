using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.PaletteSwitch
{
    public static class SelectionGroupExtensions
    {
        public static Material[] GetMaterials(this SelectionGroups.Runtime.SelectionGroup group)
        {
            var materials = new HashSet<Material>();
            foreach (var i in group.GetMemberComponents<Renderer>())
            {
                materials.UnionWith(i.sharedMaterials);
            }
            return materials.ToArray();
        }
    }
}