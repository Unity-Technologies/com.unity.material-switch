using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Unity.MaterialSwitchhttps://github.com/Unity-Technologies/com.unity.material-switch/pull/20/conflict?name=Editor%252FMaterialSwitchClipTimelineEditor.cs&ancestor_oid=ff274cdce951a6db62150276a340bfd67258c7d1&base_oid=046881014f37831cca21eabdadd2d6426e4214e0&head_oid=b6206ca4a694195f99f0312c459599dc92aa708d
{

    [CustomEditor(typeof(MaterialSwitchClip))]
    internal class MaterialSwitchClipEditor : Editor
    {
        bool showTextureProperties;
        
        void ValidatePalettePropertyMap()
        {
            // var clip = target as MaterialSwitchClip;
            //
            // var track = ((TimelineClip)clip).GetParentTrack();
            //
            // var selectionGroup = TimelineEditor.inspectedDirector.GetGenericBinding(track) as SelectionGroups.Runtime.SelectionGroup;
            // if (selectionGroup == null)
            // {
            //     Debug.LogError("Generic Binding must be a SelectionGroup.");
            //     return;
            // }
            // if (!selectionGroup.TryGetComponent<MaterialGroup>(out MaterialGroup materialPropertyGroup))
            // {
            //     materialPropertyGroup = selectionGroup.gameObject.AddComponent<MaterialGroup>();
            //     Debug.Log("Adding Material Group to Selection Group.");
            // }
            //
            // var mappedMaterials = new HashSet<int>();
            //
            // var newMap = clip.palettePropertyMap.ToList();
            // foreach (var i in clip.palettePropertyMap)
            //     mappedMaterials.Add(i.material.GetInstanceID());
            // foreach (var i in materialGroup.sharedMaterials)
            // {
            //     if(mappedMaterials.Contains(i.GetInstanceID()))
            //         continue;
            //     //newMap.Add(MaterialSwitch.);
            // }
        }

        public override void OnInspectorGUI()
        {
            // serializedObject.Update();
            if (GUILayout.Button("Refresh Materials"))
            {
                ValidatePalettePropertyMap();
                
            }
            serializedObject.Update();
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
                    var t = textureProperty.objectReferenceValue as Texture;
                    if (!t.isReadable)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("Texture is not marked as readable!", MessageType.Error);
                        if (GUILayout.Button("Fix"))
                            MakeTextureReadable(t);
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        DrawPropertyOverrideList(ppm, "showCoords", "Color Properties", "colorCoordinates", (itemProperty) =>
                        {
                            var displayNameProperty = itemProperty.FindPropertyRelative("displayName");
                            GUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField($"{displayNameProperty.stringValue}");
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Sampled Color");
                            var rect = GUILayoutUtility.GetRect(18, 18);
                            var targetValueProperty = itemProperty.FindPropertyRelative("targetValue");
                            EditorGUI.DrawRect(rect, targetValueProperty.colorValue);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("uv"));
                            GUI.enabled = textureProperty.objectReferenceValue != null;
                            var texture = ppm.FindPropertyRelative("texture").objectReferenceValue as Texture2D;
                            if (GUILayout.Button("Pick") || GUI.Button(rect, GUIContent.none, "label"))
                            {
                                rect = GUIUtility.GUIToScreenRect(rect);
                                CoordPickerWindow.Open(this, ppm.FindPropertyRelative("texture").objectReferenceValue as Texture2D, itemProperty, rect);
                            }
                            GUI.enabled = true;
                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();

                        });
                    }
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
                        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("targetValue"));
                    }
                    GUILayout.EndVertical();
                });


                GUILayout.EndVertical();

            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPropertyOverrideList(SerializedProperty ppm, string togglePropertyPath, string heading, string arrayPropertyPath, System.Action<SerializedProperty> guiMethod = null)
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
                        var itemProperty = propertyList.GetArrayElementAtIndex(j);
                        var displayNameProperty = itemProperty.FindPropertyRelative("displayName");

                        var overrideBaseValueProperty = itemProperty.FindPropertyRelative("overrideBaseValue");
                        menu.AddItem(new GUIContent(displayNameProperty.stringValue), overrideBaseValueProperty.boolValue, ToggleOverrideBaseValueProperty, itemProperty);
                        if (!overrideBaseValueProperty.boolValue) continue;
                        if (guiMethod == null)
                        {
                            GUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField($"{itemProperty.FindPropertyRelative("displayName").stringValue}");

                            EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("baseValue"));
                            EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("targetValue"), new GUIContent("New Value"));
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
            var p = property as SerializedProperty;
            var overrideBaseValueProperty = p.FindPropertyRelative("overrideBaseValue");
            overrideBaseValueProperty.boolValue = !overrideBaseValueProperty.boolValue;
            p.serializedObject.ApplyModifiedProperties();
        }

        void MakeTextureReadable(Texture texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            AssetImporter.GetAtPath(path);
            var importer = (TextureImporter)TextureImporter.GetAtPath(path);
            importer.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

    }
}
