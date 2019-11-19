using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.SelectionGroups;
using UnityEngine;

namespace Unity.PaletteSwitch
{

    [CreateAssetMenu]
    public class PaletteAsset : ScriptableObject
    {
        public PropertyChangeCollection propertyChanges = new PropertyChangeCollection();
    }
}