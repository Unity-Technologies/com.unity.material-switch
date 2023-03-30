//[Note-sin: 2023-03-29] Comment this for debugging in the editor
//#define DEBUG_SPRITE_SWITCH


using System;
using System.Collections.Generic;
using Unity.FilmInternalUtilities;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    [AddComponentMenu("")]
    [ExecuteAlways]
    internal class SpriteSwitchEventInvoker : MonoBehaviourSingleton<SpriteSwitchEventInvoker>
    {

        private List<System.Action> actions = new List<Action>();

        public static void OnLateUpdate(System.Action action)
        {
            GetOrCreateInstance().actions.Add(action);
        }

        void OnEnable() {
            SetHideFlags(gameObject);
        }
        
        void LateUpdate()
        {
            foreach (var i in actions)
            {
                i.Invoke();
            }
            actions.Clear();
        }

        private static void SetHideFlags(GameObject go) {
#if DEBUG_SPRITE_SWITCH
            go.hideFlags = HideFlags.None;
#else
            go.hideFlags = HideFlags.HideInHierarchy;
#endif
        }
    }
}