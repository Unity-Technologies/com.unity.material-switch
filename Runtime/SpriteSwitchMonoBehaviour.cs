using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    internal class SpriteSwitchMonoBehaviour : MonoBehaviour
    {
        private static SpriteSwitchMonoBehaviour _instance;

        private static SpriteSwitchMonoBehaviour Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new GameObject().AddComponent<SpriteSwitchMonoBehaviour>();
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