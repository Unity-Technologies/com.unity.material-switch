using Unity.SelectionGroups;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.PaletteSwitch
{
    [CustomPropertyDrawer(typeof(ColorChangeCollection))]
    public class ColorChangeCollectionDrawer : PropertyDrawer
    {
        ReorderableList colorChangeList;
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

            if (colorChangeList == null)
            {
                colorChangeList = new ReorderableList(property.serializedObject, itemsProperty, false, true, true, true);
                colorChangeList.drawElementCallback = DrawElement;
                colorChangeList.drawHeaderCallback += DrawListHeader;
            }
            position = EditorGUI.PrefixLabel(position, label);
            colorChangeList.DoList(position);
        }

        void ShowPopupMenu()
        {
            var menu = new GenericMenu();
            foreach(var i in SelectionGroupUtility.GetGroupNames())
                menu.AddItem(new GUIContent($"Initialize from Selection Group/{i}"), false, InitPalette, i);
            menu.AddItem(new GUIContent("Enable Property Editing"), enableEdit, () => enableEdit = !enableEdit);
            menu.ShowAsContext();
        }

        void InitPalette(object menuData)
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
                        if (p.type == MaterialProperty.PropType.Color)
                        {
                            itemsProperty.InsertArrayElementAtIndex(0);
                            var colorChange = itemsProperty.GetArrayElementAtIndex(0);
                            colorChange.FindPropertyRelative("memberNameQuery").stringValue = i.name;
                            colorChange.FindPropertyRelative("materialIndex").intValue = index;
                            colorChange.FindPropertyRelative("propertyDisplayName").stringValue = p.displayName;
                            colorChange.FindPropertyRelative("propertyName").stringValue = p.name;
                            colorChange.FindPropertyRelative("color").colorValue = UnityEngine.Random.ColorHSV(0, 1, 0.7f, 1, 0.5f, 1f);
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
            GUI.Label(rect, "Color");
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
