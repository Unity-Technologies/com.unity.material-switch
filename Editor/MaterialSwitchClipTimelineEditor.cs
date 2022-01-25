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
            
            var selectionGroup = inspectedDirector.GetGenericBinding(track) as SelectionGroups.Runtime.SelectionGroup;
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
            var asset = clip.asset as MaterialSwitchClip;
            if (asset == null)
            {
                Debug.LogError("Asset is not a PaletteSwitchClip: " + clip.asset);
                return;
            }
            var selectionGroup = TimelineEditor.inspectedDirector.GetGenericBinding(track) as SelectionGroups.Runtime.SelectionGroup;
            if (selectionGroup == null)
            {
                return;
            }
            if (!selectionGroup.TryGetComponent<MaterialGroup>(out MaterialGroup materialPropertyGroup))
            {
                materialPropertyGroup = selectionGroup.gameObject.AddComponent<MaterialGroup>();
            }
            if (materialPropertyGroup == null)
            {
                Debug.LogError("Material Group is null.");
                return;
            }
            if (asset.materialPropertiesList != null)
            {
                //This should be ok, probably from a duplicate operation.
                
                //Debug.LogError("PalettePropertyMap is already created.");
                
                return;
            }

            asset.globalMaterialProperties =
                MaterialSwitchUtility.CreateMaterialProperties(materialPropertyGroup.sharedMaterials);
            asset.materialPropertiesList = new List<MaterialProperties>(materialPropertyGroup.sharedMaterials.Length);
            for (var i = 0; i < materialPropertyGroup.sharedMaterials.Length; i++)
            {
                var ppm = MaterialSwitchUtility.CreateMaterialProperties(materialPropertyGroup.sharedMaterials[i]);
                asset.materialPropertiesList.Add(ppm);
            }
        }
    }
}
