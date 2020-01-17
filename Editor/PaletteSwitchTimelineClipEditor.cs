using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace Unity.PaletteSwitch
{
    [CustomTimelineEditor(typeof(PaletteSwitchClip))]
    internal class PaletteSwitchTimelineClipEditor : ClipEditor
    {
        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            var asset = clip.asset as PaletteSwitchClip;
            if (asset == null) {
                Debug.LogError("Asset is not a PaletteSwitchClip: " + clip.asset);
                return;
            }
            var materialPropertyGroup = TimelineEditor.inspectedDirector.GetGenericBinding(track) as MaterialPropertyGroup;
            asset.targetMaterials = new Material[materialPropertyGroup.originalMaterials.Length];
            for(var i=0; i<materialPropertyGroup.originalMaterials.Length; i++) {
                var m = asset.targetMaterials[i] = new Material(materialPropertyGroup.originalMaterials[i]);
                AssetDatabase.AddObjectToAsset(m, asset);
            }
        }
    }
}
