using UnityEditor;
using UnityEngine;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using Unity.FilmInternalUtilities;
using Unity.SelectionGroups.Runtime;
using UnityEngine.Playables;

namespace Unity.MaterialSwitch
{
    [CustomTimelineEditor(typeof(MaterialSwitchClip))]
    internal class MaterialSwitchClipTimelineEditor : ClipEditor
    {
        
        public override void OnClipChanged(TimelineClip clip) 
        {
            
            PlayableDirector inspectedDirector = TimelineEditor.inspectedDirector;
            if (null == inspectedDirector)
                return;
            
            TrackAsset track = clip.GetParentTrack();
            SelectionGroup selectionGroup = inspectedDirector.GetGenericBinding(track) as SelectionGroups.Runtime.SelectionGroup;
            if (selectionGroup == null)
                return;
            if (!selectionGroup.TryGetComponent<MaterialGroup>(out MaterialGroup materialPropertyGroup))
                materialPropertyGroup = selectionGroup.gameObject.AddComponent<MaterialGroup>();
        }


        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            var asset = clip.asset as MaterialSwitchClip;
            if (asset == null)
            {
                Debug.LogError("Asset is not a PaletteSwitchClip: " + clip.asset);
                return;
            }
            var selectionGroup = TimelineEditor.inspectedDirector.GetGenericBinding(track) as SelectionGroups.Runtime.SelectionGroup;
            if (selectionGroup == null)
            {
                Debug.LogError("Generic Binding must be a SelectionGroup.");
                return;
            }
            if (!selectionGroup.TryGetComponent<MaterialGroup>(out MaterialGroup materialPropertyGroup))
            {
                materialPropertyGroup = selectionGroup.gameObject.AddComponent<MaterialGroup>();
                Debug.Log("Adding Material Group to Selection Group.");
            }
            if (materialPropertyGroup == null)
            {
                Debug.LogError("Material Group is null.");
                return;
            }
            if (asset.palettePropertyMap != null)
            {
                Debug.LogError("PalettePropertyMap is already created.");
                return;
            }
            asset.palettePropertyMap = new PalettePropertyMap[materialPropertyGroup.sharedMaterials.Length];
            for (var i = 0; i < materialPropertyGroup.sharedMaterials.Length; i++)
            {
                var ppm = MaterialSwitchUtility.InitPalettePropertyMap(materialPropertyGroup.sharedMaterials[i]);
                asset.palettePropertyMap[i] = ppm;
            }
        }
    }
}
