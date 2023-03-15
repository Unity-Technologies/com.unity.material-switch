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