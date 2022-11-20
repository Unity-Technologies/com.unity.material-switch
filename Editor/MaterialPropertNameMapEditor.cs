using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.MaterialSwitch
{

    [CustomEditor(typeof(MaterialPropertyNameMap))]
    public class MaterialPropertyNameRemapEditor : Editor
    {
        private MaterialPropertyNameMap[] _nameMaps = null;

        private string _warningMessage = null;
        
        private void OnEnable()
        {
            _nameMaps = Resources.FindObjectsOfTypeAll<MaterialPropertyNameMap>();
        }

        bool CheckForDuplicateMaps(SerializedProperty shaderProperty)
        {
            foreach (var i in _nameMaps)
            {
                if (i.shader.GetInstanceID() == shaderProperty.objectReferenceValue.GetInstanceID())
                {
                    if (i != target)
                    {
                        if (EditorUtility.DisplayDialog("Warning", $"This shader is already mapped in {i.name}.", "Select Asset", "Cancel"))
                        {
                            Selection.activeObject = i;
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var shaderProperty = serializedObject.FindProperty(nameof(MaterialPropertyNameMap.shader));
            var nameMapProperty = serializedObject.FindProperty(nameof(MaterialPropertyNameMap.nameMap));
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(shaderProperty);
            if (EditorGUI.EndChangeCheck())
            {
                if (CheckForDuplicateMaps(shaderProperty)) return;
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