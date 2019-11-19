using Unity.SelectionGroups;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.PaletteSwitch
{
    [CustomPropertyDrawer(typeof(PropertyChangeCollection), true)]
    public class ChangeCollectionDrawer : PropertyDrawer
    {
        ReorderableList changeList;
        bool enableEdit = false;
        SerializedProperty property, itemsProperty;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var items = property.FindPropertyRelative("items");
            var count = items.arraySize;
            return (EditorGUIUtility.singleLineHeight * 5) + ((EditorGUIUtility.singleLineHeight + 3) * count);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            this.property = property;
            itemsProperty = property.FindPropertyRelative("items");

            if (changeList == null)
            {
                changeList = new ReorderableList(property.serializedObject, itemsProperty, false, true, true, true);
                changeList.drawElementCallback = DrawElement;
                changeList.drawHeaderCallback += DrawListHeader;
            }
            // position = EditorGUI.PrefixLabel(position, label);
            changeList.DoList(position);
        }

        void ShowPopupMenu()
        {
            var menu = new GenericMenu();
            foreach (var i in SelectionGroupUtility.GetGroupNames())
                menu.AddItem(new GUIContent($"Initialize from Selection Group/{i}"), false, InitChangeList, i);
            menu.AddItem(new GUIContent("Enable Property Editing"), enableEdit, () => enableEdit = !enableEdit);
            menu.ShowAsContext();
        }

        void InitChangeList(object menuData)
        {
            var groupName = menuData as string;
            itemsProperty.ClearArray();
            foreach (var i in SelectionGroupUtility.GetComponents<Renderer>(groupName))
            {
                for (var index = 0; index < i.sharedMaterials.Length; index++)
                {
                    //required to work around bug in GetMaterialProperties
                    var singleMaterial = new[] { i.sharedMaterials[index] };
                    foreach (var p in MaterialEditor.GetMaterialProperties(singleMaterial))
                    {
                        if (p.type == MaterialProperty.PropType.Color
                        || p.type == MaterialProperty.PropType.Float
                        || p.type == MaterialProperty.PropType.Range
                        || p.type == MaterialProperty.PropType.Vector)
                        {
                            itemsProperty.InsertArrayElementAtIndex(0);
                            var change = itemsProperty.GetArrayElementAtIndex(0);
                            change.FindPropertyRelative("memberNameQuery").stringValue = i.name;
                            change.FindPropertyRelative("materialIndex").intValue = index;
                            change.FindPropertyRelative("propertyDisplayName").stringValue = p.displayName;
                            change.FindPropertyRelative("propertyName").stringValue = p.name;
                            var typeProperty = change.FindPropertyRelative("propertyType");
                            switch (p.type)
                            {
                                case MaterialProperty.PropType.Color:
                                    change.FindPropertyRelative("colorValue").colorValue = UnityEngine.Random.ColorHSV(0, 1, 0.7f, 1, 0.5f, 1f);
                                    typeProperty.intValue = PropertyChange.COLOR;
                                    break;
                                case MaterialProperty.PropType.Float:
                                case MaterialProperty.PropType.Range:
                                    change.FindPropertyRelative("floatValue").floatValue = 0;
                                    typeProperty.intValue = PropertyChange.FLOAT;
                                    break;
                                case MaterialProperty.PropType.Vector:
                                    typeProperty.intValue = PropertyChange.VECTOR;
                                    change.FindPropertyRelative("vectorValue").vector4Value = Vector4.zero;
                                    break;
                            }
                        }
                    }
                }
            }
            itemsProperty.serializedObject.ApplyModifiedProperties();
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
            GUI.Label(rect, "Value");
            rect.x = rect.xMax - 16;
            rect.width = 16;
            if (EditorGUI.DropdownButton(rect, EditorGUIUtility.IconContent("_Popup"), FocusType.Passive, EditorStyles.label))
            {
                ShowPopupMenu();
            }
        }

        void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height = EditorGUIUtility.singleLineHeight;
            var property = changeList.serializedProperty.GetArrayElementAtIndex(index);
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
                var typePropertyID = property.FindPropertyRelative("propertyType").intValue;
                switch (typePropertyID)
                {
                    case PropertyChange.VECTOR:
                        EditorGUI.PropertyField(rect, property.FindPropertyRelative("vectorValue"), GUIContent.none);
                        break;
                    case PropertyChange.FLOAT:
                        EditorGUI.PropertyField(rect, property.FindPropertyRelative("floatValue"), GUIContent.none);
                        break;
                    case PropertyChange.COLOR:
                        EditorGUI.PropertyField(rect, property.FindPropertyRelative("colorValue"), GUIContent.none);
                        break;
                }
            }
        }
    }
}
