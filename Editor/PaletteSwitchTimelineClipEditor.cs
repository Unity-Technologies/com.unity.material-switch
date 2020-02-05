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
            if (asset == null)
            {
                Debug.LogError("Asset is not a PaletteSwitchClip: " + clip.asset);
                return;
            }
            var materialPropertyGroup = TimelineEditor.inspectedDirector.GetGenericBinding(track) as MaterialPropertyGroup;
            if (materialPropertyGroup == null)
                return;
            if(asset.palettePropertyMap != null)
                return;
            asset.palettePropertyMap = new PalettePropertyMap[materialPropertyGroup.sharedMaterials.Length];
            for (var i = 0; i < materialPropertyGroup.sharedMaterials.Length; i++)
            {
                var ppm = asset.palettePropertyMap[i] = new PalettePropertyMap() { material = materialPropertyGroup.sharedMaterials[i] };
                var materialProperties = MaterialEditor.GetMaterialProperties(new[] { ppm.material });
                foreach (var mp in materialProperties)
                {
                    if (mp.type == MaterialProperty.PropType.Color)
                    {
                        ppm.colorCoordinates.Add(
                            new ColorCoordinate()
                            {
                                uv = Vector2.zero,
                                propertyName = mp.displayName,
                                propertyId = Shader.PropertyToID(mp.name),
                                sampledColor = Color.clear,
                                originalColor = mp.colorValue
                            }
                        );
                    }
                }
            }
        }
    }
}
