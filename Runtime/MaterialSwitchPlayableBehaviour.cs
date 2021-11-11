using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.MaterialSwitch
{
    internal class MaterialSwitchPlayableBehaviour : PlayableBehaviour
    {
        public List<MaterialProperties> palettePropertyMap;
        public MaterialSwitchClip clip;

        // This magic method is only available in the editor.
        internal static System.Func<Material[], MaterialProperties> CreatePalettePropertyMap;

        // public PalettePropertyMap GetGlobalMap()
        // {
        //     return clip.globalPalettePropertyMap;
        // }
        //
        // public PalettePropertyMap GetMap(Material material)
        // {
        //     foreach (var i in clip.palettePropertyMap)
        //     {
        //         if (i.material.GetInstanceID() == material.GetInstanceID()) return i;
        //     }
        //     if (CreatePalettePropertyMap != null)
        //     {
        //         var ppm = CreatePalettePropertyMap(new [] { material });
        //         ppm.material = material;
        //         clip.palettePropertyMap.Add(ppm);
        //         
        //         return ppm;
        //     }
        //     
        //     return null;
        // }

    }
}