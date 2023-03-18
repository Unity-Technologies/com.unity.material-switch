//[Note-sin: 2023-03-18] Comment this for debugging in the editor
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

        protected override void AwakeInternalV() { 
#if DEBUG_SPRITE_SWITCH
            gameObject.hideFlags = HideFlags.None;
#else
            gameObject.hideFlags = HideFlags.HideInHierarchy;
#endif
        }

        public static void OnLateUpdate(System.Action action)
        {
            GetOrCreateInstance().actions.Add(action);
        }

        void LateUpdate()
        {
            foreach (var i in actions)
            {
                i.Invoke();
            }
            actions.Clear();
        }

    }
}