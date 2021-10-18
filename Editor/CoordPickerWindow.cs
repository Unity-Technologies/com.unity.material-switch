using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.MaterialSwitch
{
    internal class CoordPickerWindow : EditorWindow
    {
        Texture2D texture;
        SerializedProperty ccProperty;
        SerializedProperty sampledColorProperty;
        SerializedProperty uvProperty;
        MaterialSwitchClipEditor inspector;

        public static void Open(MaterialSwitchClipEditor inspector, Texture2D texture, SerializedProperty cc, Rect rect)
        {
            var window = ScriptableObject.CreateInstance<CoordPickerWindow>();
            window.inspector = inspector;
            window.texture = texture;
            window.ccProperty = cc;
            window.sampledColorProperty = cc.FindPropertyRelative("targetValue");
            window.uvProperty = cc.FindPropertyRelative("uv");
            // window.ShowModalUtility(); //<-- HAHA LOL Modal is not modal. :facepalm:
            window.ShowAsDropDown(rect, new Vector2(texture.width, texture.height));
        }

        void OnGUI()
        {
            if (sampledColorProperty == null)
            {
                Close(); 
                return;
            }
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
                    uvProperty.vector2Value = uv;
                    //sampledColorProperty.colorValue = texture.GetPixel((int)uv.x, (int)uv.y);
                    ccProperty.serializedObject.ApplyModifiedProperties();
                    Debug.Log(ccProperty.GetHashCode());
                    foreach(var t in Resources.FindObjectsOfTypeAll<PlayableDirector>()) {
                        t.DeferredEvaluate();
                    }
                    Close();
                    break;
                case EventType.MouseUp:
                    break;
            }
        }

        
    }
}
