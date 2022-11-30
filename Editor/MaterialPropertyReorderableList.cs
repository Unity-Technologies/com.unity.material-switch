using System;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    public class MaterialPropertyReorderableList : ReorderableList
    {
        private string _filterText = "";

        public MaterialPropertyReorderableList(SerializedObject serializedObject, SerializedProperty elements, string filter) : base(serializedObject, elements, draggable:false, displayHeader:true, displayAddButton:false, displayRemoveButton:false)
        {
            this.drawElementCallback = DrawElement;
            this.elementHeightCallback = ElementHeight;
            this.drawHeaderCallback = DrawHeader;
            _filterText = filter;
        }

        private void DrawRow(Rect rect, System.Action<Rect> col1, System.Action<Rect> col2, System.Action<Rect> col3)
        {
            var cursor = rect;
            cursor.width = rect.width * 0.45f;
            col1(cursor);
            cursor.x += cursor.width;
            cursor.width = rect.width * 0.1f;
            EditorGUIUtility.labelWidth = 48;
            col2(cursor);
            cursor.x += cursor.width;
            cursor.width = rect.width * 0.45f;
            col3(cursor);
        }

        private void DrawHeader(Rect rect)
        {
            DrawRow(rect,
                (r) => EditorGUI.LabelField(r, "Property", EditorStyles.boldLabel),
                (r) => EditorGUI.LabelField(r, "Hide", EditorStyles.boldLabel),
                (r) => EditorGUI.LabelField(r, "Display Name", EditorStyles.boldLabel)
            );
        }

        private float ElementHeight(int i)
        {
            //if we are not filtering results
            if (string.IsNullOrEmpty(_filterText))
                return EditorGUIUtility.singleLineHeight;
                
            // a filter has been specified, check that the display name matches, else return 0 to skip rendering.
            var query = _filterText.ToLower();
            var property = this.serializedProperty.GetArrayElementAtIndex(i);
            if (property.displayName.ToLower().Contains(query))
            {
                return EditorGUIUtility.singleLineHeight;
            }
            return 0;
        }

        private void DrawElement(Rect rect, int i, bool isactive, bool isfocused)
        {
            // if rect height is 0, this item has been filtered out and should not be drawn.
            if (rect.height == 0) return;
            var property = this.serializedProperty.GetArrayElementAtIndex(i);
            property.isExpanded = true;
            var hiddenProperty = property.FindPropertyRelative(nameof(MaterialPropertyNameMap.PropertyDisplayName.hidden));
            var displayNameProperty = property.FindPropertyRelative(nameof(MaterialPropertyNameMap.PropertyDisplayName.displayName));

            DrawRow(rect,
                (r) => EditorGUI.LabelField(r, property.displayName, EditorStyles.boldLabel),
                (r) => EditorGUI.PropertyField(r, hiddenProperty, GUIContent.none),
                (r) => EditorGUI.PropertyField(r, displayNameProperty, GUIContent.none)
            );
        }

    }
}