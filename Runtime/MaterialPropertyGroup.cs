using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.PaletteSwitch
{
    [ExecuteAlways]
    [RequireComponent(typeof(SelectionGroups.Runtime.SelectionGroup))]
    public class MaterialPropertyGroup : MonoBehaviour
    {
        public Material[] sharedMaterials;

        Dictionary<Material, MaterialPropertyBlock> materialPropertyBlocks = new Dictionary<Material, MaterialPropertyBlock>();

        public MaterialPropertyBlock GetMaterialPropertyBlock(Material material)
        {
            return materialPropertyBlocks[material];
        }

        void OnEnable()
        {
            foreach (var i in sharedMaterials)
            {
                materialPropertyBlocks[i] = new MaterialPropertyBlock();
            }
        }

        void Reset()
        {
            var group = GetComponent<SelectionGroups.Runtime.SelectionGroup>();
            var materials = new HashSet<Material>();
            foreach (var i in group.GetMemberComponents<Renderer>())
            {
                materials.UnionWith(i.sharedMaterials);
            }
            this.sharedMaterials = materials.ToArray();
        }
    }
}