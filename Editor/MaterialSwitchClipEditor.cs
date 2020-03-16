using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.MaterialSwitch
{

    [CustomEditor(typeof(MaterialSwitchClip))]
    public class MaterialSwitchClipEditor : Editor
    {
        bool showTextureProperties;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var palettePropertyMap = serializedObject.FindProperty("palettePropertyMap");
            for (var i = 0; i < palettePropertyMap.arraySize; i++)
            {
                var ppm = palettePropertyMap.GetArrayElementAtIndex(i);
                GUILayout.BeginVertical();
                EditorGUILayout.PropertyField(ppm.FindPropertyRelative("material"));
                var textureProperty = ppm.FindPropertyRelative("texture");
                EditorGUILayout.PropertyField(textureProperty);
                var showCoords = ppm.FindPropertyRelative("showCoords");
                showCoords.boolValue = EditorGUILayout.Foldout(showCoords.boolValue, "Color Coordinates");
                if (showCoords.boolValue)
                {
                    GUILayout.BeginVertical("box");

                    var ccs = ppm.FindPropertyRelative("colorCoordinates");
                    for (var j = 0; j < ccs.arraySize; j++)
                    {
                        var cc = ccs.GetArrayElementAtIndex(j);
                        GUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"Property: {cc.FindPropertyRelative("propertyName").stringValue}");
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Sampled Color");
                        var rect = GUILayoutUtility.GetRect(18, 18);
                        EditorGUI.DrawRect(rect, cc.FindPropertyRelative("sampledColor").colorValue);
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(cc.FindPropertyRelative("uv"));
                        GUI.enabled = textureProperty.objectReferenceValue != null;
                        if (GUILayout.Button("Pick"))
                        {
                            CoordPickerWindow.Open(this, ppm.FindPropertyRelative("texture").objectReferenceValue as Texture2D, cc);
                        }
                        GUI.enabled = true;
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();

                }

                showTextureProperties = EditorGUILayout.Foldout(showTextureProperties, "Texture Properties");
                if (showTextureProperties)
                {
                    GUILayout.BeginVertical("box");
                    var textureProperties = ppm.FindPropertyRelative("textureProperties");
                    if (textureProperties != null)
                        for (var j = 0; j < textureProperties.arraySize; j++)
                        {
                            var tp = textureProperties.GetArrayElementAtIndex(j);
                            GUILayout.BeginVertical("box");
                            EditorGUILayout.LabelField($"Property: {tp.FindPropertyRelative("propertyName").stringValue}");

                            EditorGUILayout.PropertyField(tp.FindPropertyRelative("originalValue"));
                            EditorGUILayout.PropertyField(tp.FindPropertyRelative("targetValue"), new GUIContent("New Value"));
                            GUILayout.EndVertical();
                        }
                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();

            }

            // EditorGUILayout.PropertyField(palettePropertyMap);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
