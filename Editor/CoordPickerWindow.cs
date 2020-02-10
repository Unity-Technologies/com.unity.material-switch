using UnityEditor;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    public class CoordPickerWindow : EditorWindow
    {
        Texture2D texture;
        SerializedProperty ccProperty;
        SerializedProperty sampledColorProperty;
        SerializedProperty uvProperty;
        MaterialSwitchClipEditor inspector;

        public static void Open(MaterialSwitchClipEditor inspector, Texture2D texture, SerializedProperty cc)
        {
            var window = ScriptableObject.CreateInstance<CoordPickerWindow>();
            window.inspector = inspector;
            window.texture = texture;
            window.ccProperty = cc;
            window.sampledColorProperty = cc.FindPropertyRelative("sampledColor");
            window.uvProperty = cc.FindPropertyRelative("uv");
            window.ShowModalUtility(); //<-- HAHA LOL Modal is not modal. :facepalm:
        }

        void OnGUI()
        {
            position = new Rect(position.x, position.y, texture.width, texture.height);
            var rect = position;
            rect.x = 0;
            rect.y = 0;
            GUI.DrawTexture(rect, texture);
            var e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDrag:
                case EventType.MouseDown:
                    var uv = e.mousePosition;
                    uv.y = texture.height - uv.y;
                    sampledColorProperty.colorValue = texture.GetPixel((int)uv.x, (int)uv.y);
                    uvProperty.vector2Value = uv;
                    inspector.serializedObject.ApplyModifiedProperties();
                    inspector.Repaint();
                    break;
                case EventType.MouseUp:
                    break;
            }
        }

    }
}
