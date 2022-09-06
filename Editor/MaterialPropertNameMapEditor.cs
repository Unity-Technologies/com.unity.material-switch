using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.MaterialSwitch
{

    [CustomEditor(typeof(MaterialPropertyNameMap))]
    public class MaterialPropertNameRemapEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var materialProperty = serializedObject.FindProperty(nameof(MaterialPropertyNameMap.material));
            var nameMapProperty = serializedObject.FindProperty(nameof(MaterialPropertyNameMap.nameMap));
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(materialProperty);
            if (EditorGUI.EndChangeCheck())
            {
                if (materialProperty.objectReferenceValue == null)
                {
                    nameMapProperty.ClearArray();
                }
                else
                {
                    nameMapProperty.ClearArray();
                    var materialProperties = MaterialEditor.GetMaterialProperties(new[] {materialProperty.objectReferenceValue});
                    foreach (var mp in materialProperties)
                    {
                        nameMapProperty.InsertArrayElementAtIndex(0);
                        var p = nameMapProperty.GetArrayElementAtIndex(0);
                        var propertyNameProperty = p.FindPropertyRelative(nameof(MaterialPropertyNameMap.PropertyDisplayName.propertyName));
                        propertyNameProperty.stringValue = mp.name;
                        var displayNameProperty = p.FindPropertyRelative(nameof(MaterialPropertyNameMap.PropertyDisplayName.displayName));
                        displayNameProperty.stringValue = mp.displayName;
                    }
                }
            }
            
            EditorGUILayout.PropertyField(nameMapProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}