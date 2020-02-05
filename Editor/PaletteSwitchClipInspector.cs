using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.PaletteSwitch
{
    public class CoordPickerWindow : EditorWindow
    {
        Texture2D texture;
        SerializedProperty ccProperty;
        SerializedProperty sampledColorProperty;
        SerializedProperty uvProperty;
        PaletteSwitchClipInspector inspector;

        public static void Open(PaletteSwitchClipInspector inspector, Texture2D texture, SerializedProperty cc)
        {
            var window = ScriptableObject.CreateInstance<CoordPickerWindow>();
            window.inspector = inspector;
            window.texture = texture;
            window.ccProperty = cc;
            window.sampledColorProperty = cc.FindPropertyRelative("sampledColor");
            window.uvProperty = cc.FindPropertyRelative("uv");
            window.ShowModalUtility(); //<-- HAHA LOL Modal is not modal. :facepalm:
        }

        void OnGUI()
        {
            position = new Rect(position.x, position.y, texture.width, texture.height);
            var rect = position;
            rect.x = 0;
            rect.y = 0;
            GUI.DrawTexture(rect, texture);
            var e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDrag:
                case EventType.MouseDown:
                    var uv = e.mousePosition;
                    uv.y = texture.height - uv.y;
                    sampledColorProperty.colorValue = texture.GetPixel((int)uv.x, (int)uv.y);
                    uvProperty.vector2Value = uv;
                    inspector.serializedObject.ApplyModifiedProperties();
                    inspector.Repaint();
                    break;
                case EventType.MouseUp:
                    break;
            }
        }

    }

    [CustomEditor(typeof(PaletteSwitchClip))]
    public class PaletteSwitchClipInspector : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var palettePropertyMap = serializedObject.FindProperty("palettePropertyMap");
            for (var i = 0; i < palettePropertyMap.arraySize; i++)
            {
                var ppm = palettePropertyMap.GetArrayElementAtIndex(i);
                GUILayout.BeginVertical();
                EditorGUILayout.PropertyField(ppm.FindPropertyRelative("material"));
                EditorGUILayout.PropertyField(ppm.FindPropertyRelative("texture"));
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
                        if (GUILayout.Button("Pick"))
                        {
                            CoordPickerWindow.Open(this, ppm.FindPropertyRelative("texture").objectReferenceValue as Texture2D, cc);
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();

                }
                GUILayout.EndVertical();

            }

            EditorGUILayout.PropertyField(palettePropertyMap);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
