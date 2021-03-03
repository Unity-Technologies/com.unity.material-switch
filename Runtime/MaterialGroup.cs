using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    [ExecuteAlways]
    [RequireComponent(typeof(SelectionGroups.Runtime.SelectionGroup))]
    internal class MaterialGroup : MonoBehaviour
    {
        public Material[] sharedMaterials;

        void OnEnable()
        {
            CollectMaterials();
        }

        public void CollectMaterials()
        {
            var group = GetComponent<SelectionGroups.Runtime.SelectionGroup>();
            var materials = new HashSet<Material>();
            foreach (var i in group.GetMemberComponents<Renderer>())
            {
                materials.UnionWith(i.sharedMaterials);
            }
            sharedMaterials = materials.ToArray();
        }
    }
}