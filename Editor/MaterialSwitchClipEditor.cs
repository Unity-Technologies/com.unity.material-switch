using System;
using System.Collections.Generic;
using System.Linq;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitch
{
    [CustomEditor(typeof(MaterialSwitchClip))]
    [CanEditMultipleObjects]
    internal class MaterialSwitchClipEditor : Editor
    {
        bool showTextureProperties;

        private static MaterialPropertiesClipboardData copyClipboardData;

        private string _errorMessage = null;

        private HashSet<Material> activeMaterials = new HashSet<Material>();
        
        private GUIContent _settingsIcon;
        private GUIContent _duplicateIcon;
        private GUIContent _trashIcon;
        private GUIStyle _iconButtonStyle;
        private RemapNameCache _remapNameCache;

        void UpdateSampledColors()
        {
            var clip = target as MaterialSwitchClip;
            var globalTexture = clip.globalMaterialProperties.texture;
            foreach (var map in clip.materialPropertiesList)
            {
                var textureToUse = map.texture == null ? globalTexture : map.texture;
                if (textureToUse == null) continue;
                if (!textureToUse.isReadable) continue;
                foreach (var c in map.colorProperties)
                {
                    c.targetValue = textureToUse.GetPixel((int) c.uv.x, (int) c.uv.y);
                }
            }
        }

        void HandleContextClick(SerializedProperty property, int targetIndex)
        {
            var e = Event.current;
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy Settings"), false, () => { copyClipboardData = MaterialPropertiesClipboardData.Create(target as MaterialSwitchClip, targetIndex); });
            if (copyClipboardData == null)
                menu.AddDisabledItem(new GUIContent("Paste Settings"));
            else
                menu.AddItem(new GUIContent("Paste Settings"), false, () =>
                {
                    var targetClip = target as MaterialSwitchClip;
                    copyClipboardData.PasteInto(targetClip, targetIndex);
                    serializedObject.ApplyModifiedProperties();
                });

            menu.ShowAsContext();
            e.Use();
        }

        private void OnEnable()
        {
            _settingsIcon = EditorGUIUtility.IconContent("d_SettingsIcon");
            _settingsIcon.tooltip = "Settings";
            _duplicateIcon = EditorGUIUtility.IconContent("TreeEditor.Duplicate", "Copy original material color.");
            _trashIcon = EditorGUIUtility.IconContent("TreeEditor.Trash", "Remove override.");
            _remapNameCache = new RemapNameCache();
            errorMessage = null;
            //when the editor is enabled, get the target clip and make sure it is up to date.

            PlayableDirector inspectedDirector = TimelineEditor.inspectedDirector;
            if (inspectedDirector == null)
            {
                _errorMessage = "Could not find Timeline Director.";
                return;
            }

            var selectedClip = TimelineEditor.selectedClip;
            if (selectedClip == null)
            {
                _errorMessage = "Could not find Selected Clip.";
                return;
            }

            var track = selectedClip.GetParentTrack();
            var selectionGroup = inspectedDirector.GetGenericBinding(track) as SelectionGroups.SelectionGroup;
            if (selectionGroup == null)
            {
                _errorMessage = "No Selection Group is bound to the Track.";
                return;
            }

            if (!selectionGroup.TryGetComponent(out MaterialGroup materialGroup))
            {
                materialGroup = selectionGroup.gameObject.AddComponent<MaterialGroup>();
            }

            materialGroup.CollectMaterials();

            var asset = target as MaterialSwitchClip;

            if (asset.globalMaterialProperties == null || asset.globalMaterialProperties.needsUpdate)
            {
                asset.globalMaterialProperties = MaterialSwitchEditorUtility.CreateMaterialProperties(materialGroup.sharedMaterials);
            }

            activeMaterials = new HashSet<Material>(materialGroup.sharedMaterials);
            var storedMaterials = new HashSet<Material>(from i in asset.materialPropertiesList select i.material);
            var missingMaterials = activeMaterials.Except(storedMaterials).ToArray();
            if (missingMaterials.Length > 0)
            {
                _errorMessage = $"Created {missingMaterials.Length} new material maps. These will be saved in the clip asset.";
                foreach (var material in missingMaterials)
                {
                    var materialProperties = MaterialSwitchEditorUtility.CreateMaterialProperties(material);
                    asset.materialPropertiesList.Add(materialProperties);
                }

                EditorUtility.SetDirty(asset);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // This has to be done here as we cannot use GUI.skin outside of an OnGUI function.
            if(_iconButtonStyle == null)
                _iconButtonStyle = GUI.skin.FindStyle("IconButton") ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("IconButton");

            if (!string.IsNullOrEmpty(_errorMessage))
            {
                EditorGUILayout.HelpBox(_errorMessage, MessageType.Warning);
            }

            GUILayout.BeginVertical("box");

            var globalPalettePropertyMap = serializedObject.FindProperty(nameof(MaterialSwitchClip.globalMaterialProperties));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Global Properties");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(_settingsIcon, EditorStyles.toolbarDropDown))
            {
                //negative targetIndex is reserved for global properties
                HandleContextClick(globalPalettePropertyMap, -1);
            }

            GUILayout.EndHorizontal();

            //negative targetIndex is reserved for global properties
            DrawPalettePropertyMapUI(globalPalettePropertyMap, null, -1);

            EditorGUI.indentLevel += 1;
            EditorGUILayout.EndVertical();
            GUILayout.Space(16);
            GUILayout.BeginVertical("box");
            GUILayout.Label("Per Material Properties");

            var materialPropertiesList = serializedObject.FindProperty(nameof(MaterialSwitchClip.materialPropertiesList));
            for (var i = 0; i < materialPropertiesList.arraySize; i++)
            {
                var property = materialPropertiesList.GetArrayElementAtIndex(i);
                var materialProperty = property.FindPropertyRelative(nameof(MaterialProperties.material));
                var material = materialProperty.objectReferenceValue as Material;
                if (activeMaterials.Contains(material))
                {
                    DrawPalettePropertyMapUI(property, globalPalettePropertyMap, i);
                }
            }

            GUILayout.EndVertical();

            if (serializedObject.ApplyModifiedProperties())
            {
                UpdateSampledColors();
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawPalettePropertyMapUI(SerializedProperty ppm, SerializedProperty globalPalettePropertyMap, int index)
        {
            SerializedProperty materialProperty = null;
            GUILayout.BeginVertical("box");
            EditorGUI.indentLevel--;
            SerializedProperty materialProperty = ppm.FindPropertyRelative(nameof(MaterialProperties.material));;

            if (globalPalettePropertyMap != null)
            {
                //This is a per material ppm, so draw the material field.
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(materialProperty, GUILayout.MinWidth(384));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(_settingsIcon, EditorStyles.toolbarDropDown))
                {
                    HandleContextClick(ppm, index);
                }

                GUILayout.EndHorizontal();
            }

            var textureProperty = ppm.FindPropertyRelative(nameof(MaterialProperties.texture));
            var globalTextureProperty = globalPalettePropertyMap?.FindPropertyRelative(nameof(MaterialProperties.texture));

            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(textureProperty, new GUIContent("Palette Texture"));
            if (textureProperty.objectReferenceValue != null)
            {
                var t = textureProperty.objectReferenceValue as Texture2D;
                if (!t.isReadable)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("Texture is not marked as readable!", MessageType.Error);
                    if (GUILayout.Button("Fix")) MakeTextureReadable(t);
                    GUILayout.EndHorizontal();
                }
            }

            DrawPropertyOverrideList(ppm, "showCoords", "Color Properties", "colorProperties", (itemProperty, displayName) =>
                {
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(displayName);
                GUILayout.FlexibleSpace();
                ShowRemoveOverrideButton(itemProperty);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Sampled Color");
                var texture = ppm.FindPropertyRelative(nameof(MaterialProperties.texture)).objectReferenceValue as Texture2D;
                var globalPaletteTexture = globalTextureProperty?.objectReferenceValue as Texture2D;
                var targetValueProperty = itemProperty.FindPropertyRelative(nameof(ColorProperty.targetValue));
                if (texture == null && globalPaletteTexture == null)
                {
                    EditorGUILayout.PropertyField(targetValueProperty, GUIContent.none);
                    // If there is no material, we cannot sample the original color.
                    if(materialProperty != null)
                    {
                       
                        if (GUILayout.Button(_duplicateIcon, _iconButtonStyle, GUILayout.Width(22)))
                        {
                            var material = materialProperty.objectReferenceValue as Material;
                            var propertyName = itemProperty.FindPropertyRelative(nameof(ColorProperty.propertyName)).stringValue;
                            var color = material.GetColor(propertyName);
                            targetValueProperty.colorValue = color;
                        }
                    }
                }
                else
                {
                    var rect = GUILayoutUtility.GetRect(18, 18);
                    EditorGUI.DrawRect(rect, targetValueProperty.colorValue);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative(nameof(ColorProperty.uv)));
                    var textureToUse = texture == null ? globalPaletteTexture : texture;
                    GUI.enabled = textureToUse != null;
                    if (GUILayout.Button("Pick") || GUI.Button(rect, GUIContent.none, "label"))
                    {
                        rect = GUIUtility.GUIToScreenRect(rect);
                        CoordPickerWindow.Open(textureToUse, itemProperty, rect);
                    }
                }

                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            });

            DrawPropertyOverrideList(ppm, "showTextures", "Texture Properties", "textureProperties");
            DrawPropertyOverrideList(ppm, "showFloats", "Float Properties", "floatProperties", (itemProperty, displayName) =>
            {
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(displayName);
                GUILayout.FlexibleSpace();
                ShowRemoveOverrideButton(itemProperty);
                GUILayout.EndHorizontal();
                //EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative(nameof(FloatProperty.baseValue)));
                var limits = itemProperty.FindPropertyRelative(nameof(RangeProperty.rangeLimits));
                if (limits != null)
                {
                    var targetValueProperty = itemProperty.FindPropertyRelative(nameof(FloatProperty.targetValue));
                    var minmax = limits.vector2Value;
                    var originalValue = targetValueProperty.floatValue;
                    var newValue = EditorGUILayout.Slider(originalValue, minmax.x, minmax.y);
                    if (originalValue != newValue)
                    {
                        targetValueProperty.floatValue = newValue;
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative(nameof(FloatProperty.targetValue)));
                }

                GUILayout.EndVertical();
            });

            GUILayout.EndVertical();
        }

        private void ShowRemoveOverrideButton(SerializedProperty itemProperty)
        {
            if (GUILayout.Button(_trashIcon, _iconButtonStyle))
            {
                var overrideBaseValueProperty = itemProperty.FindPropertyRelative(nameof(MaterialSwitchProperty.overrideBaseValue));
                overrideBaseValueProperty.boolValue = false;
            }
        }

        private void DrawPropertyOverrideList(SerializedProperty ppm, string togglePropertyPath, string heading, string arrayPropertyPath, System.Action<SerializedProperty, string> guiMethod = null)
        {
            var showPropertyListToggle = ppm.FindPropertyRelative(togglePropertyPath);
            showPropertyListToggle.boolValue = EditorGUILayout.Foldout(showPropertyListToggle.boolValue, heading);
            
            if (showPropertyListToggle.boolValue)
            {
                var materialProperty = ppm.FindPropertyRelative(nameof(MaterialProperties.material));
                var material = materialProperty.objectReferenceValue as Material;
                var propertyList = ppm.FindPropertyRelative(arrayPropertyPath);
                if (propertyList != null)
                {
                    GUILayout.BeginVertical("box");
                    var menu = new GenericMenu();

                    for (var j = 0; j < propertyList.arraySize; j++)
                    {
                        var itemProperty = propertyList.GetArrayElementAtIndex(j);
                        var propertyName = itemProperty.FindPropertyRelative(nameof(ColorProperty.propertyName)).stringValue;
                        var (displayName, isHidden) = _remapNameCache.GetDisplayName(material, propertyName);
                        if (isHidden)
                            continue;
                        var overrideBaseValueProperty = itemProperty.FindPropertyRelative(nameof(MaterialSwitchProperty.overrideBaseValue));
                        menu.AddItem(new GUIContent(displayName), overrideBaseValueProperty.boolValue, ToggleOverrideBaseValueProperty, itemProperty);
                        if (!overrideBaseValueProperty.boolValue) continue;
                        if (guiMethod == null)
                        {
                            GUILayout.BeginVertical("box");
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(displayName);
                            GUILayout.FlexibleSpace();
                            ShowRemoveOverrideButton(itemProperty);
                            GUILayout.EndHorizontal();
                            EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("targetValue"), new GUIContent("New Value"));
                            GUILayout.EndVertical();
                        }
                        else
                        {
                            guiMethod(itemProperty, displayName);
                        }
                    }

                    ShowDropDownButton(menu, $"Choose {heading} Overrides");
                    GUILayout.EndVertical();
                }
            }
        }

        void ShowDropDownButton(GenericMenu menu, string buttonLabel)
        {
            var buttonText = new GUIContent(buttonLabel);
            var buttonRect = GUILayoutUtility.GetRect(buttonText, "button");
            if (GUI.Button(buttonRect, buttonText))
            {
                menu.DropDown(buttonRect);
            }
        }

        void ToggleOverrideBaseValueProperty(object property)
        {
            var p = property as SerializedProperty;
            var overrideBaseValueProperty = p.FindPropertyRelative("overrideBaseValue");
            overrideBaseValueProperty.boolValue = !overrideBaseValueProperty.boolValue;
            p.serializedObject.ApplyModifiedProperties();
        }

        void MakeTextureReadable(Texture texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            AssetImporter.GetAtPath(path);
            var importer = (TextureImporter) TextureImporter.GetAtPath(path);
            importer.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            UpdateSampledColors();
        }

        
    }
}