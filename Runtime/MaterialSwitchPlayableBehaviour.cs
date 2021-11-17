using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.MaterialSwitch
{
    internal class MaterialSwitchPlayableBehaviour : PlayableBehaviour
    {
        public List<MaterialProperties> materialPropertiesList;
        public MaterialSwitchClip clip;

        // This magic method is only available in the editor.
        internal static System.Func<Material[], MaterialProperties> CreateMaterialProperties;

   
    }
}