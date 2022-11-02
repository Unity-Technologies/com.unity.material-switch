using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.MaterialSwitch
{

    [CustomEditor(typeof(MaterialPropertyNameMap))]
    public class MaterialPropertyNameRemapEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var shaderProperty = serializedObject.FindProperty(nameof(MaterialPropertyNameMap.shader));
            var nameMapProperty = serializedObject.FindProperty(nameof(MaterialPropertyNameMap.nameMap));
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(shaderProperty);
            if (EditorGUI.EndChangeCheck())
            {
                if (shaderProperty.objectReferenceValue == null)
                {
                    nameMapProperty.ClearArray();
                }
                else
                {
                    nameMapProperty.ClearArray();
                    var materialProperties = GetMaterialProperties(shaderProperty.objectReferenceValue);
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

        private IEnumerable<MaterialProperty> GetMaterialProperties(Object obj)
        {
            if (obj is Shader shader)
            {
                var m = new Material(shader);
                foreach(var mp in MaterialEditor.GetMaterialProperties(new[] {m}))
                {
                    yield return mp;
                }
                DestroyImmediate(m);
            }
        }
    }
}