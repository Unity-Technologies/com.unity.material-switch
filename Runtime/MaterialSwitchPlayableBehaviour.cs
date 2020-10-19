using UnityEngine;
using UnityEngine.Playables;

namespace Unity.MaterialSwitch
{
    internal class MaterialSwitchPlayableBehaviour : PlayableBehaviour
    {
        public PalettePropertyMap[] palettePropertyMap;
        public PalettePropertyMap GetMap(Material material)
        {
            foreach (var i in palettePropertyMap)
            {
                if (i.material.GetInstanceID() == material.GetInstanceID()) return i;
            }
            return null;
        }
    }
}