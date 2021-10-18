﻿using UnityEditor;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    [CustomEditor(typeof(MaterialSwitchClip))]
    [CanEditMultipleObjects]
    internal class MaterialSwitchClipEditor : Editor
    {
        bool              showTextureProperties;
        private static MaterialSwitchClip copySource;

        void UpdateSampledColors()
        {
            var clip          = target as MaterialSwitchClip;
            var globalTexture = clip.globalPalettePropertyMap.texture;
            foreach (var map in clip.palettePropertyMap)
            {
                var textureToUse = map.texture == null ? globalTexture : map.texture;
                if (textureToUse == null) continue;
                if(!textureToUse.isReadable) continue;
                foreach (var c in map.colorCoordinates)
                {
                    c.targetValue = textureToUse.GetPixel((int) c.uv.x, (int) c.uv.y);
                }
            }
        }

        void HandleContextClick()
        {
            var e = Event.current;
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy Settings"), false, () =>
            {
                copySource = Instantiate(target) as MaterialSwitchClip;
            });
            if(copySource == null)
                menu.AddDisabledItem(new GUIContent("Paste Settings"));
            else
                menu.AddItem(new GUIContent("Paste Settings"), false, () =>
                {
                    EditorUtility.CopySerializedManagedFieldsOnly(copySource, target);
                    serializedObject.ApplyModifiedProperties();
                });
            
            menu.ShowAsContext();
            e.Use();
        }

        public override void OnInspectorGUI()
        {
            if (Event.current.type == EventType.ContextClick)
                HandleContextClick();
            serializedObject.Update();
            
            
            
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("Global Properties");
            var globalPalettePropertyMap = serializedObject.FindProperty(nameof(MaterialSwitchClip.globalPalettePropertyMap));
            DrawPalettePropertyMapUI(globalPalettePropertyMap, null);                        

            EditorGUI.indentLevel += 1;
            EditorGUILayout.EndVertical();
            GUILayout.Space(16);
            GUILayout.BeginVertical("box");
            GUILayout.Label("Per Material Properties");
            var palettePropertyMap = serializedObject.FindProperty(nameof(MaterialSwitchClip.palettePropertyMap));
            
            for (var i = 0; i < palettePropertyMap.arraySize; i++)
            {
                var ppm = palettePropertyMap.GetArrayElementAtIndex(i);
                DrawPalettePropertyMapUI(ppm, globalPalettePropertyMap);
            }
            GUILayout.EndVertical();

            if (serializedObject.ApplyModifiedProperties())
            {
                UpdateSampledColors();
                serializedObject.ApplyModifiedProperties();
            }
        }
        
        private void DrawPalettePropertyMapUI(SerializedProperty ppm, SerializedProperty globalPalettePropertyMap)
        {
            GUILayout.BeginVertical("box");
            EditorGUI.indentLevel--;
            if (globalPalettePropertyMap != null)
            {
                //This is a per material ppm, so draw the material field.
                EditorGUILayout.PropertyField(ppm.FindPropertyRelative(nameof(PalettePropertyMap.material)));
            }

            var textureProperty = ppm.FindPropertyRelative(nameof(PalettePropertyMap.texture));
            var globalTextureProperty = globalPalettePropertyMap?.FindPropertyRelative(nameof(PalettePropertyMap.texture));
            
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(textureProperty, new GUIContent("Palette Texture"));
            if (textureProperty.objectReferenceValue != null)
            {
                var t = textureProperty.objectReferenceValue as Texture2D;
                if (!t.isReadable)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("Texture is not marked as readable!", MessageType.Error);
                    if (GUILayout.Button("Fix"))
                        MakeTextureReadable(t);
                    GUILayout.EndHorizontal();
                }
            }


            DrawPropertyOverrideList(ppm, "showCoords", "Color Properties", "colorCoordinates",
                (itemProperty) =>
                {
                    var displayNameProperty = itemProperty.FindPropertyRelative(nameof(ColorProperty.displayName));
                    GUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"{displayNameProperty.stringValue}");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Sampled Color");
                    var texture = ppm.FindPropertyRelative(nameof(PalettePropertyMap.texture)).objectReferenceValue as Texture2D;
                    var globalPaletteTexture = globalTextureProperty?.objectReferenceValue as Texture2D;
                    if (texture == null && globalPaletteTexture == null)
                    {
                        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative(nameof(ColorProperty.targetValue)),
                            GUIContent.none);
                    }
                    else
                    {
                        var rect = GUILayoutUtility.GetRect(18, 18);
                        var targetValueProperty = itemProperty.FindPropertyRelative(nameof(ColorProperty.targetValue));
                        EditorGUI.DrawRect(rect, targetValueProperty.colorValue);
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        var uvProperty = itemProperty.FindPropertyRelative(nameof(ColorProperty.uv));
                        EditorGUILayout.PropertyField(uvProperty);
                        var textureToUse = texture == null ? globalPaletteTexture : texture;
                        GUI.enabled = textureToUse != null;
                        var uv = uvProperty.vector2Value;
                        targetValueProperty.colorValue = textureToUse.GetPixel((int)uv.x, (int)uv.y);
                        if (GUILayout.Button("Pick") || GUI.Button(rect, GUIContent.none, "label"))
                        {
                            rect = GUIUtility.GUIToScreenRect(rect);
                            CoordPickerWindow.Open(this, textureToUse, itemProperty, rect);
                        }
                    }

                    GUI.enabled = true;
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                });


            DrawPropertyOverrideList(ppm, "showTextures", "Texture Properties", "textureProperties");
            DrawPropertyOverrideList(ppm, "showFloats", "Float Properties", "floatProperties", (itemProperty) =>
            {
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(
                    $"{itemProperty.FindPropertyRelative(nameof(FloatProperty.displayName)).stringValue}");
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

        private void DrawPropertyOverrideList(SerializedProperty ppm, string togglePropertyPath, string heading,
            string arrayPropertyPath, System.Action<SerializedProperty> guiMethod = null)
        {
            var show = ppm.FindPropertyRelative(togglePropertyPath);
            show.boolValue = EditorGUILayout.Foldout(show.boolValue, heading);
            if (show.boolValue)
            {
                var propertyList = ppm.FindPropertyRelative(arrayPropertyPath);
                if (propertyList != null)
                {
                    GUILayout.BeginVertical("box");
                    var menu = new GenericMenu();

                    for (var j = 0; j < propertyList.arraySize; j++)
                    {
                        var itemProperty        = propertyList.GetArrayElementAtIndex(j);
                        var displayNameProperty = itemProperty.FindPropertyRelative(nameof(MaterialSwitchProperty.displayName));

                        var overrideBaseValueProperty = itemProperty.FindPropertyRelative(nameof(MaterialSwitchProperty.overrideBaseValue));
                        menu.AddItem(new GUIContent(displayNameProperty.stringValue),
                            overrideBaseValueProperty.boolValue, ToggleOverrideBaseValueProperty, itemProperty);
                        if (!overrideBaseValueProperty.boolValue) continue;
                        if (guiMethod == null)
                        {
                            GUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField(
                                $"{displayNameProperty.stringValue}");

                            //EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("baseValue"));
                            EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("targetValue"),
                                new GUIContent("New Value"));
                            GUILayout.EndVertical();
                        }
                        else
                        {
                            guiMethod(itemProperty);
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
            var p                         = property as SerializedProperty;
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