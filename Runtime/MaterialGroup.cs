using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Unity.MaterialSwitch
{
    [ExecuteAlways]
    [RequireComponent(typeof(SelectionGroups.SelectionGroup))]
    internal class MaterialGroup : MonoBehaviour
    {
        public Material[] sharedMaterials;

        void OnEnable()
        {
            CollectMaterials();
#if UNITY_EDITOR
            EditorApplication.hierarchyChanged -= CollectMaterials;
            EditorApplication.hierarchyChanged += CollectMaterials;
#endif
        }
        
        void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.hierarchyChanged -= CollectMaterials;
#endif
        }

        public void CollectMaterials()
        {
            var group = GetComponent<SelectionGroups.SelectionGroup>();
            var materials = new HashSet<Material>();
            foreach (var i in group.GetMemberComponents<Renderer>())
            {
                materials.UnionWith(i.sharedMaterials);
            }
            sharedMaterials = materials.ToArray();
        }
    }
}