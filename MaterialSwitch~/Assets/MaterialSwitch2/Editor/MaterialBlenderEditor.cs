using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


[CustomEditor(typeof(MaterialBlender))]
public class MaterialBlenderEditor : Editor
{
    private Editor sourceEditor, targetEditor, blendedEditor;
    private float blendValue = 0;

    private void OnEnable()
    {
        ValidateEditors();
    }

    private void ValidateMaterials()
    {
        var blender = target as MaterialBlender;
        var sourceMaterial = blender.sourceMaterial;
        if (sourceMaterial != null)
        {
            blender.propertyKeys = new List<MaterialBlender.RuntimeMaterialProperty>();
            foreach (var mp in MaterialEditor.GetMaterialProperties(new[] {blender.sourceMaterial}))
            {
                blender.propertyKeys.Add(new MaterialBlender.RuntimeMaterialProperty() { name = mp.name, type = Enum.Parse<MaterialBlender.PropType>(mp.type.ToString())});
            }
            if (blender.targetMaterial == null)
                blender.targetMaterial = CreateMaterial("Target");
            if (blender.blendedMaterial == null)
                blender.blendedMaterial = CreateMaterial("Blended");
            ValidateEditors();
        }

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
    }

    private void ValidateEditors()
    {
        var blender = target as MaterialBlender;
        BuildEditor(blender.sourceMaterial, ref sourceEditor);
        BuildEditor(blender.targetMaterial, ref targetEditor);
        BuildEditor(blender.blendedMaterial, ref blendedEditor);
    }

    Material CreateMaterial(string suffix)
    {
        var blender = (MaterialBlender) target;

        var newMaterial = Instantiate(blender.sourceMaterial);
        newMaterial.name = $"{blender.sourceMaterial.name}-{suffix}";
        AssetDatabase.AddObjectToAsset(newMaterial, blender);
        AssetDatabase.SaveAssets();
        return newMaterial;
    }

    void BuildEditor(UnityEngine.Object editorTarget, ref Editor editor)
    {
        if (editor != null)
            DestroyImmediate(editor);
        if (editorTarget != null)
            editor = CreateEditor(editorTarget, typeof(BlendedMaterialEditor));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        EditorGUI.BeginChangeCheck();
        var sourceMaterialProperty = serializedObject.FindProperty(nameof(MaterialBlender.sourceMaterial));
        EditorGUI.BeginDisabledGroup(sourceMaterialProperty.objectReferenceValue != null);
        EditorGUILayout.PropertyField(sourceMaterialProperty);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(MaterialBlender.targetMaterial)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(MaterialBlender.blendedMaterial)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(MaterialBlender.propertyKeys)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(MaterialBlender.blend)));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            ValidateMaterials();
        }

        EditorGUI.BeginChangeCheck();
        foreach(var e in new [] { sourceEditor, targetEditor, blendedEditor })
        {
            if (e == null) continue;
            e.DrawHeader();
            e.OnInspectorGUI();
        }

        var blender = target as MaterialBlender;
        if (EditorGUI.EndChangeCheck() || blender.blend != blendValue)
        {
            blendValue = blender.blend;
            blender.InterpolateMaterialProperties();
        }
    }

    private void OnDestroy()
    {
        foreach(var e in new [] { sourceEditor, targetEditor, blendedEditor })
        {
            if (e == null) continue;
            DestroyImmediate(e);
        }
    }

    
}
