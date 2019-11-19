using UnityEngine;
using UnityEngine.Playables;

namespace Unity.PaletteSwitch
{
    public class PaletteSwitchBehaviour : PlayableBehaviour
    {
        public PaletteAsset paletteAsset;
        public PropertyChangeCollection propertyOverrides;
    }
}