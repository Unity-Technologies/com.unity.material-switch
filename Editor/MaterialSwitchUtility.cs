using System;
using JetBrains.Annotations;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitch
{
    /// <summary>
    /// Utility class to perform various functions on MaterialSwitch classes.
    /// </summary>
    [Obsolete("Replaced by MaterialSwitchEditorUtility")] 
    public static class MaterialSwitchUtility
    {
        /// <summary>
        /// Init a MaterialSwitchClip 
        /// </summary>
        /// <param name="clip">The clip to be initialized</param>
        [CanBeNull]
        [Obsolete("Replaced by MaterialSwitchEditorUtility.InitMaterialSwitchClip")] 
        public static MaterialSwitchClip InitMaterialSwitchClip(TimelineClip clip, TrackAsset track) {
            return MaterialSwitchEditorUtility.InitMaterialSwitchClip(clip, track);
        }       
    }
}