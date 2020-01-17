using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.PaletteSwitch
{
    [RequireComponent(typeof(SelectionGroups.Runtime.SelectionGroup))]
    public class MaterialPropertyGroup : MonoBehaviour
    {
        public Material[] sharedMaterials;
        public Material[] originalMaterials;

        void Reset()
        {
            var group = GetComponent<SelectionGroups.Runtime.SelectionGroup>();
            var materials = new HashSet<Material>();
            foreach (var i in group.GetMemberComponents<Renderer>())
            {
                materials.UnionWith(i.sharedMaterials);
            }
            this.sharedMaterials = materials.ToArray();
            originalMaterials = new Material[this.sharedMaterials.Length];
            for (var i = 0; i < this.sharedMaterials.Length; i++)
            {
                originalMaterials[i] = new Material(this.sharedMaterials[i]);
            }
        }

        [ContextMenu("Restore Original Materials")]
        public void RestoreOriginalMaterials()
        {
            for (var i = 0; i < sharedMaterials.Length; i++)
            {
                sharedMaterials[i].CopyPropertiesFromMaterial(originalMaterials[i]);
            }
        }

        public void LerpTowards(Material[] targetMaterials, float v)
        {
            for (var i = 0; i < sharedMaterials.Length; i++)
            {
                sharedMaterials[i].Lerp(sharedMaterials[i], targetMaterials[i], v);
            }
        }
    }
}