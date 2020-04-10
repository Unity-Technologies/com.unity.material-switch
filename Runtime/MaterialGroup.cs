using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    [ExecuteAlways]
    [RequireComponent(typeof(SelectionGroups.Runtime.SelectionGroup))]
    public class MaterialGroup : MonoBehaviour
    {
        public Material[] sharedMaterials;

        Dictionary<Material, MaterialPropertyBlock> materialPropertyBlocks = new Dictionary<Material, MaterialPropertyBlock>();

        public bool isDirty = true;

        public MaterialPropertyBlock GetMaterialPropertyBlock(Material material)
        {
            if (materialPropertyBlocks.TryGetValue(material, out MaterialPropertyBlock mpb))
                return mpb;
            return null;
        }

        void OnEnable()
        {
            CollectMaterials();
        }

        void Update()
        {
            if (isDirty)
            {
                isDirty = false;
                CollectMaterials();
            }
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

            if (sharedMaterials != null)
            {
                foreach (var i in sharedMaterials)
                {
                    materialPropertyBlocks[i] = new MaterialPropertyBlock();
                }
            }
        }
    }
}