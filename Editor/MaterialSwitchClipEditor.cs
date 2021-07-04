using UnityEditor;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    [CustomEditor(typeof(MaterialSwitchClip))]
    internal class MaterialSwitchClipEditor : Editor
    {
        bool              showTextureProperties;

        void UpdateSampledColors()
        {
            var clip          = target as MaterialSwitchClip;
            var globalTexture = clip.globalTexture;
            foreach (var map in clip.palettePropertyMap)
            {
                var textureToUse = map.texture == null ? globalTexture : map.texture;
                if (textureToUse == null) continue;
                foreach (var c in map.colorCoordinates)
                {
                    c.targetValue = textureToUse.GetPixel((int) c.uv.x, (int) c.uv.y);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            var globalTextureProperty = serializedObject.FindProperty("globalTexture");
            
            GUILayout.BeginVertical("box");
            EditorGUI.indentLevel--;
            GUILayout.Label("Global");
            EditorGUILayout.PropertyField(globalTextureProperty, new GUIContent("Global Palette Texture"));
            var globalPaletteTexture = globalTextureProperty.objectReferenceValue as Texture2D;
            if (globalPaletteTexture != null && !globalPaletteTexture.isReadable)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Texture is not marked as readable!", MessageType.Error);
                if (GUILayout.Button("Fix")) 
                    MakeTextureReadable(globalPaletteTexture);
                GUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel += 1;
            EditorGUILayout.EndVertical();

            SerializedProperty palettePropertyMap = serializedObject.FindProperty("palettePropertyMap");
            for (var i = 0; i < palettePropertyMap.arraySize; i++)
            {
                var ppm = palettePropertyMap.GetArrayElementAtIndex(i);
                GUILayout.BeginVertical("box");
                EditorGUI.indentLevel--;
                EditorGUILayout.PropertyField(ppm.FindPropertyRelative("material"));
                var textureProperty = ppm.FindPropertyRelative("texture");
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

                if (globalPaletteTexture != null || textureProperty.objectReferenceValue != null)
                {
                    DrawPropertyOverrideList(ppm, "showCoords", "Color Properties", "colorCoordinates",
                        (itemProperty) =>
                        {
                            var displayNameProperty = itemProperty.FindPropertyRelative("displayName");
                            GUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField($"{displayNameProperty.stringValue}");
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Sampled Color");
                            var rect                = GUILayoutUtility.GetRect(18, 18);
                            var targetValueProperty = itemProperty.FindPropertyRelative("targetValue");
                            EditorGUI.DrawRect(rect, targetValueProperty.colorValue);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("uv"));
                            var texture = ppm.FindPropertyRelative("texture").objectReferenceValue as Texture2D;

                            var textureToUse = texture == null ? globalPaletteTexture:texture;
                            GUI.enabled = textureToUse != null;
                            if (GUILayout.Button("Pick") || GUI.Button(rect, GUIContent.none, "label"))
                            {
                                rect = GUIUtility.GUIToScreenRect(rect);
                                CoordPickerWindow.Open(this, textureToUse, itemProperty, rect);
                            }

                            GUI.enabled = true;
                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                        });
                }


                DrawPropertyOverrideList(ppm, "showTextures", "Texture Properties", "textureProperties");
                DrawPropertyOverrideList(ppm, "showFloats", "Float Properties", "floatProperties", (itemProperty) =>
                {
                    GUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"{itemProperty.FindPropertyRelative("displayName").stringValue}");
                    EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("baseValue"));
                    var limits = itemProperty.FindPropertyRelative("rangeLimits");
                    if (limits != null)
                    {
                        var targetValueProperty = itemProperty.FindPropertyRelative("targetValue");
                        var minmax              = limits.vector2Value;
                        var originalValue       = targetValueProperty.floatValue;
                        var newValue            = EditorGUILayout.Slider(originalValue, minmax.x, minmax.y);
                        if (originalValue != newValue)
                        {
                            targetValueProperty.floatValue = newValue;
                        }
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("targetValue"));
                    }

                    GUILayout.EndVertical();
                });


                GUILayout.EndVertical();
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                UpdateSampledColors();
                serializedObject.ApplyModifiedProperties();
            }
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
                        var displayNameProperty = itemProperty.FindPropertyRelative("displayName");

                        var overrideBaseValueProperty = itemProperty.FindPropertyRelative("overrideBaseValue");
                        menu.AddItem(new GUIContent(displayNameProperty.stringValue),
                            overrideBaseValueProperty.boolValue, ToggleOverrideBaseValueProperty, itemProperty);
                        if (!overrideBaseValueProperty.boolValue) continue;
                        if (guiMethod == null)
                        {
                            GUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField(
                                $"{itemProperty.FindPropertyRelative("displayName").stringValue}");

                            EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("baseValue"));
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