using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.PaletteSwitch
{
    [CustomEditor(typeof(PaletteSwitchClip))]
    public class PaletteSwitchClipInspector : Editor
    {
        Dictionary<Object, Editor> materialEditors = new Dictionary<Object, Editor>();

        void OnDisable()
        {
            foreach (var e in materialEditors.Values)
            {
                Editor.DestroyImmediate(e);
            }
            materialEditors.Clear();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var ev = Event.current;
            var targetMaterials = serializedObject.FindProperty("targetMaterials");

            for (var i = 0; i < targetMaterials.arraySize; i++)
            {
                var p = targetMaterials.GetArrayElementAtIndex(i);
                if (p.objectReferenceValue == null) continue;
                if (!materialEditors.TryGetValue(p.objectReferenceValue, out Editor editor))
                {
                    editor = materialEditors[p.objectReferenceValue] = Editor.CreateEditor(p.objectReferenceValue);
                }
                editor.DrawHeader();
                editor.OnInspectorGUI();
                GUILayout.Space(16);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
