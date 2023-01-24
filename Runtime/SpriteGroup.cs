using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.MaterialSwitch
{
    [ExecuteAlways]
    [RequireComponent(typeof(SelectionGroups.SelectionGroup))]
    internal class SpriteGroup : MonoBehaviour
    {
        public SpriteRenderer[] spriteRenderers;
        public List<Sprite> defaultSprites;

        void OnEnable()
        {
            CollectSpriteRenderers();
#if UNITY_EDITOR
            EditorApplication.hierarchyChanged -= CollectSpriteRenderers;
            EditorApplication.hierarchyChanged += CollectSpriteRenderers;
#endif
        }
        
        void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.hierarchyChanged -= CollectSpriteRenderers;
#endif
        }

        public void CollectSpriteRenderers()
        {
            var group = GetComponent<SelectionGroups.SelectionGroup>();
            spriteRenderers = group.GetMemberComponents<SpriteRenderer>().ToArray();
            defaultSprites ??= new List<Sprite>(spriteRenderers.Length);
            defaultSprites.Clear();
            foreach (var sr in spriteRenderers)
            {
                defaultSprites.Add(sr.sprite);    
            }
        }
    }
}