using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.SelectionGroups;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.PaletteSwitch
{
    [CustomPropertyDrawer(typeof(ColorChange))]
    public class ColorChangeDrawer : PropertyDrawer
    {
        GUIContent displayLabel = new GUIContent();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 3;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rect = position;
            var width = position.width;
            rect.y += 1f;
            rect.width = width * 0.375f;
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("memberNameQuery"), GUIContent.none);
            rect.x += rect.width;
            rect.width = width * 0.125f;
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("materialIndex"), GUIContent.none);
            displayLabel = new GUIContent(string.Empty);
            displayLabel.text = property.FindPropertyRelative("propertyDisplayName").stringValue;
            displayLabel.tooltip = property.FindPropertyRelative("propertyName").stringValue;
            rect.x += rect.width;
            rect.width = width * 0.3f;
            // EditorGUI.LabelField(rect, displayLabel);
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("propertyName"), GUIContent.none);
            rect.x += rect.width;
            rect.width = width * 0.2f;
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("color"), GUIContent.none);
        }
    }

    [CustomEditor(typeof(PaletteAsset))]
    public class PaletteAssetInspector : Editor
    {

        ReorderableList colorChangeList;
        bool enableEdit = false;

        public override void OnInspectorGUI()
        {
            var paletteAsset = target as PaletteAsset;
            var paletteProperty = serializedObject.FindProperty("palette");

            if (colorChangeList == null)
            {
                colorChangeList = new ReorderableList(serializedObject, paletteProperty, false, true, true, true);
                colorChangeList.drawElementCallback = DrawElement;
                colorChangeList.drawHeaderCallback += DrawListHeader;
            }
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("groupName"), GUILayout.ExpandWidth(true));
            if (EditorGUILayout.DropdownButton(EditorGUIUtility.IconContent("_Popup"), FocusType.Passive, EditorStyles.label, GUILayout.ExpandWidth(false)))
            {
                ShowPopupMenu(paletteAsset, paletteProperty);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            colorChangeList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void ShowPopupMenu(PaletteAsset paletteAsset, SerializedProperty paletteProperty)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Initialize from Selection Group"), false, () => InitPalette(paletteAsset, paletteProperty));
            menu.AddItem(new GUIContent("Enable Property Editing"), enableEdit, () => enableEdit = !enableEdit);
            menu.ShowAsContext();
        }

        static void InitPalette(PaletteAsset paletteAsset, SerializedProperty paletteProperty)
        {
            paletteProperty.ClearArray();
            foreach (var i in SelectionGroupUtility.GetComponents<Renderer>(paletteAsset.groupName))
            {
                for (var index = 0; index < i.sharedMaterials.Length; index++)
                {
                    //required to work around bug in GetMaterialProperties
                    var singleMaterial = new[] { i.sharedMaterials[index] };
                    foreach (var p in MaterialEditor.GetMaterialProperties(singleMaterial))
                    {
                        if (p.type == MaterialProperty.PropType.Color)
                        {
                            paletteProperty.InsertArrayElementAtIndex(0);
                            var colorChange = paletteProperty.GetArrayElementAtIndex(0);
                            colorChange.FindPropertyRelative("memberNameQuery").stringValue = i.name;
                            colorChange.FindPropertyRelative("materialIndex").intValue = index;
                            colorChange.FindPropertyRelative("propertyDisplayName").stringValue = p.displayName;
                            colorChange.FindPropertyRelative("propertyName").stringValue = p.name;
                            colorChange.FindPropertyRelative("color").colorValue = UnityEngine.Random.ColorHSV(0, 1, 0.7f, 1, 0.5f, 1f);
                        }
                    }
                }
            }
            paletteProperty.serializedObject.ApplyModifiedProperties();
        }

        void DrawListHeader(Rect rect)
        {
            var width = rect.width;
            rect.width = width * 0.375f;
            GUI.Label(rect, "Name");
            rect.x += rect.width;
            rect.width = width * 0.125f;
            GUI.Label(rect, "Material");
            rect.x += rect.width;
            rect.width = width * 0.3f;
            GUI.Label(rect, "Property");
            rect.x += rect.width;
            rect.width = width * 0.2f;
            GUI.Label(rect, "Color");
        }

        void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            var property = colorChangeList.serializedProperty.GetArrayElementAtIndex(index);
            if (isActive && enableEdit)
                EditorGUI.PropertyField(rect, property);
            else
            {
                var width = rect.width;
                rect.y += 1f;
                rect.width = width * 0.375f;
                EditorGUI.LabelField(rect, property.FindPropertyRelative("memberNameQuery").stringValue);
                rect.x += rect.width;
                rect.width = width * 0.125f;
                EditorGUI.LabelField(rect, property.FindPropertyRelative("materialIndex").intValue.ToString());
                rect.x += rect.width;
                rect.width = width * 0.3f;
                // EditorGUI.LabelField(rect, displayLabel);
                EditorGUI.LabelField(rect, property.FindPropertyRelative("propertyDisplayName").stringValue);
                rect.x += rect.width;
                rect.width = width * 0.2f;
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("color"), GUIContent.none);
            }
        }
    }
}