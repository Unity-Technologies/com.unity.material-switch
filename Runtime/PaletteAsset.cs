using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.SelectionGroups;
using UnityEngine;

namespace Unity.PaletteSwitch
{

    [CreateAssetMenu]
    public class PaletteAsset : ScriptableObject
    {
        [SelectionGroup] public string groupName;

        public ColorChange[] palette;

        public bool GetColorChange(string uid, out ColorChange cc)
        {
            foreach (var p in palette)
            {
                if (p.UID == uid)
                {
                    cc = p;
                    return true;
                }
            }
            cc = new ColorChange() { color = Color.clear };
            return false;
        }

        public static bool GetDefaultColor(SelectionGroup group, ColorChange p, out Color color)
        {
            var renderers = SelectionGroupUtility.GetComponents<Renderer>(group.name);
            foreach (var r in renderers)
            {
                if (Match(p.memberNameQuery, r.name))
                {
                    color = r.sharedMaterials[p.materialIndex].GetColor(p.propertyName);
                    return true;
                }
            }
            color = Color.white;
            return false;
        }

        static public void ClearPropertyBlock(SelectionGroup group)
        {
            foreach (var r in SelectionGroupUtility.GetComponents<Renderer>(group.name))
            {
                r.SetPropertyBlock(null);
            }
        }

        public static void SetPropertyBlock(SelectionGroup group, IEnumerable<ColorChange> colorChanges)
        {
            var renderers = SelectionGroupUtility.GetComponents<Renderer>(group.name).ToArray();
            foreach (var r in renderers)
            {
                var mpb = new MaterialPropertyBlock();
                foreach (var p in colorChanges)
                {
                    if (Match(p.memberNameQuery, r.name))
                        mpb.SetColor(p.propertyName, p.color);
                }
                r.SetPropertyBlock(mpb);
            }
        }

        static bool Match(string query, string name)
        {
            var endsWith = query.EndsWith("*");
            var startsWith = query.StartsWith("*");
            if (endsWith && startsWith)
                return name.Contains(query.Substring(1, query.Length - 2));
            if (startsWith)
                return name.StartsWith(query.Substring(1));
            if (endsWith)
                return name.EndsWith(query.Substring(0, query.Length - 1));
            return query == name;
        }
    }
}