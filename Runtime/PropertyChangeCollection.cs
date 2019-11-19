using System.Collections.Generic;
using System.Linq;
using Unity.SelectionGroups;
using UnityEngine;

namespace Unity.PaletteSwitch
{
    [System.Serializable]
    public class PropertyChangeCollection
    {
        [SelectionGroup] public string groupName;

        public PropertyChange[] items;

        internal static void ClearPropertyBlock(SelectionGroup group)
        {
            foreach (var r in SelectionGroupUtility.GetComponents<Renderer>(group.name))
            {
                r.SetPropertyBlock(null);
            }
        }

        internal static bool Match(string query, string name)
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

        // public bool GetColorChange(string uid, out PropertyChange cc)
        // {
        //     foreach (var p in items)
        //     {
        //         if (p.UID == uid)
        //         {
        //             cc = p;
        //             return true;
        //         }
        //     }
        //     cc = new PropertyChange();
        //     return false;
        // }

        public static bool GetDefaultColor(SelectionGroup group, PropertyChange p, out Color color)
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

        public static bool GetDefaultFloat(SelectionGroup group, PropertyChange p, out float value)
        {
            var renderers = SelectionGroupUtility.GetComponents<Renderer>(group.name);
            foreach (var r in renderers)
            {
                if (Match(p.memberNameQuery, r.name))
                {
                    value = r.sharedMaterials[p.materialIndex].GetFloat(p.propertyName);
                    return true;
                }
            }
            value = 0;
            return false;
        }

        public static bool GetDefaultVector(SelectionGroup group, PropertyChange p, out Vector4 vector)
        {
            var renderers = SelectionGroupUtility.GetComponents<Renderer>(group.name);
            foreach (var r in renderers)
            {
                if (Match(p.memberNameQuery, r.name))
                {
                    vector = r.sharedMaterials[p.materialIndex].GetVector(p.propertyName);
                    return true;
                }
            }
            vector = Vector4.zero;
            return false;
        }

        public static void SetPropertyBlock(SelectionGroup group, IEnumerable<PropertyChange> changes)
        {
            var renderers = SelectionGroupUtility.GetComponents<Renderer>(group.name).ToArray();
            foreach (var r in renderers)
            {
                var mpb = new MaterialPropertyBlock();
                foreach (var p in changes)
                {
                    if (Match(p.memberNameQuery, r.name))
                        switch (p.propertyType)
                        {
                            case PropertyChange.VECTOR:
                                mpb.SetVector(p.propertyName, p.vectorValue);
                                break;
                            case PropertyChange.COLOR:
                                mpb.SetColor(p.propertyName, p.colorValue);
                                break;
                            case PropertyChange.FLOAT:
                                mpb.SetFloat(p.propertyName, p.floatValue);
                                break;
                        }
                }
                r.SetPropertyBlock(mpb);
            }
        }

    }
}