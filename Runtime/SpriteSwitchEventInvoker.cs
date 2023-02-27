using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    [AddComponentMenu("")]
    [ExecuteAlways]
    internal class SpriteSwitchEventInvoker : MonoBehaviour
    {
        private static SpriteSwitchEventInvoker _instance;

        private static SpriteSwitchEventInvoker Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new GameObject().AddComponent<SpriteSwitchEventInvoker>();
                    _instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
                }
                return _instance;
            }
        }

        private List<System.Action> actions = new List<Action>();

        public static void OnLateUpdate(System.Action action)
        {
            Instance.actions.Add(action);
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