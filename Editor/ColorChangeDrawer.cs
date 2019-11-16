using UnityEditor;
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
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("propertyName"), GUIContent.none);
            rect.x += rect.width;
            rect.width = width * 0.2f;
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("color"), GUIContent.none);
        }
    }
}