using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class BlendedMaterialEditor : MaterialEditor
{
    private new bool ShouldEditorBeHidden()
    {
        return false;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}