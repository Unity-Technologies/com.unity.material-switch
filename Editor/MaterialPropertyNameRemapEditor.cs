using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.MaterialSwitch
{
    [CustomEditor(typeof(MaterialPropertyNameMap))]
    public class MaterialPropertyNameRemapEditor : Editor
    {
        private Dictionary<int, MaterialPropertyNameMap> _nameMaps = new Dictionary<int, MaterialPropertyNameMap>();

        private MaterialPropertyReorderableList _propertyList;

        private string _filterText;
        
        private void OnEnable()
        {
            var assets  = Resources.FindObjectsOfTypeAll<MaterialPropertyNameMap>();
            foreach(var asset in assets) {
                if (null == asset.shader)
                    continue;
                int shaderID = asset.shader.GetInstanceID();
                _nameMaps[shaderID] = asset;
            }
        }

        bool CheckForDuplicateMaps(SerializedProperty shaderProperty)
        {
            if (_nameMaps.TryGetValue(shaderProperty.objectReferenceValue.GetInstanceID(), out var existing )) {
                if (existing == target) return false; 
                if (EditorUtility.DisplayDialog("Warning", $"This shader is already mapped in {existing.name}.", "Select Asset", "Cancel"))
                {
                    Selection.activeObject = existing;
                }
                return true;                
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
                //bookkeeping
                int shaderID = shaderProperty.objectReferenceValue.GetInstanceID();
                _nameMaps.Remove(shaderID);
                _nameMaps[shaderID] = target as MaterialPropertyNameMap;
                
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
            EditorGUI.BeginChangeCheck();
            _filterText = EditorGUILayout.TextField("Search", _filterText);
            if (EditorGUI.EndChangeCheck())
            {
                // the list needs to be recreated with a filter, as it cannot be filtered dynamically due to to caching in the ReorderableList class.
                _propertyList = new MaterialPropertyReorderableList(serializedObject, nameMapProperty, _filterText);
            }
            else
            {
                _propertyList ??= new MaterialPropertyReorderableList(serializedObject, nameMapProperty, filter:null);
            }
            _propertyList.DoLayoutList();
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