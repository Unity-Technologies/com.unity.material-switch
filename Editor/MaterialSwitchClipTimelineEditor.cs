using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitch
{
    [CustomTimelineEditor(typeof(MaterialSwitchClip))]
    internal class MaterialSwitchClipTimelineEditor : ClipEditor
    {
        public override void OnClipChanged(TimelineClip clip)
        {
            var track = clip.parentTrack;
            var selectionGroup = TimelineEditor.inspectedDirector.GetGenericBinding(track) as SelectionGroups.Runtime.SelectionGroup;
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
                var ppm = asset.palettePropertyMap[i] = new PalettePropertyMap() { material = materialPropertyGroup.sharedMaterials[i] };
                var materialProperties = MaterialEditor.GetMaterialProperties(new[] { ppm.material });
                foreach (var mp in materialProperties)
                {
                    if (mp.flags.HasFlag(MaterialProperty.PropFlags.HideInInspector))
                        continue;
                    if (mp.flags.HasFlag(MaterialProperty.PropFlags.PerRendererData))
                        continue;

                    if (mp.type == MaterialProperty.PropType.Color)
                    {
                        ppm.colorCoordinates.Add(
                            new ColorProperty()
                            {
                                uv = Vector2.zero,
                                displayName = mp.displayName,
                                propertyName = mp.name,
                                targetValue = Color.clear,
                                baseValue = mp.colorValue
                            }
                        );
                    }

                    if (mp.type == MaterialProperty.PropType.Texture)
                    {
                        ppm.textureProperties.Add(
                            new TextureProperty()
                            {
                                displayName = mp.displayName,
                                propertyName = mp.name,
                                propertyId = Shader.PropertyToID(mp.name),
                                baseValue = (Texture2D)mp.textureValue
                            }
                        );
                    }

                    if (mp.type == MaterialProperty.PropType.Float)
                    {
                        ppm.floatProperties.Add(
                            new FloatProperty()
                            {
                                displayName = mp.displayName,
                                propertyName = mp.name,
                                propertyId = Shader.PropertyToID(mp.name),
                                baseValue = mp.floatValue,
                                targetValue = mp.floatValue
                            }
                        );
                    }
                    if (mp.type == MaterialProperty.PropType.Range)
                    {
                        ppm.floatProperties.Add(
                            new RangeProperty()
                            {
                                displayName = mp.displayName,
                                propertyName = mp.name,
                                propertyId = Shader.PropertyToID(mp.name),
                                baseValue = mp.floatValue,
                                targetValue = mp.floatValue,
                                rangeLimits = mp.rangeLimits
                            }
                        );
                    }
                }
            }
        }
    }
}
