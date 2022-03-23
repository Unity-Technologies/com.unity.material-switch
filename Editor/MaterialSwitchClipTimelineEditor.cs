using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using Unity.FilmInternalUtilities;
using Unity.SelectionGroups;
using UnityEngine.Playables;

namespace Unity.MaterialSwitch
{
    [CustomTimelineEditor(typeof(MaterialSwitchClip))]
    internal class MaterialSwitchClipTimelineEditor : ClipEditor
    {
        
         
       
        
        public override void OnClipChanged(TimelineClip clip) 
        {
            
            PlayableDirector inspectedDirector = TimelineEditor.inspectedDirector;
            if (inspectedDirector == null)
                return;
               
            TrackAsset track = clip.GetParentTrack();
            
            SelectionGroup selectionGroup = inspectedDirector.GetGenericBinding(track) as SelectionGroups.SelectionGroup;
            if (selectionGroup == null)
                return;
            if (!selectionGroup.TryGetComponent(out MaterialGroup materialGroup))
                materialGroup = selectionGroup.gameObject.AddComponent<MaterialGroup>();
            
            var asset = clip.asset as MaterialSwitchClip;
            
            if (asset.globalMaterialProperties == null || asset.globalMaterialProperties.needsUpdate)
            {
                asset.globalMaterialProperties =
                    MaterialSwitchUtility.CreateMaterialProperties(materialGroup.sharedMaterials);
            }
        }

        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            MaterialSwitchUtility.InitMaterialSwitchClip(clip);
        }
    }
}
