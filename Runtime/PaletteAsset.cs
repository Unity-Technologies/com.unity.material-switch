using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.SelectionGroups;
using UnityEngine;

namespace Unity.PaletteSwitch
{

    [System.Serializable]
    public class ColorChange
    {
        public bool enabled;
        public string memberNameQuery;
        public int materialIndex;
        public string propertyName;
        public string propertyDisplayName;
        public Color color;
    }

    [CreateAssetMenu]
    public class PaletteAsset : ScriptableObject
    {
        [SelectionGroup] public string groupName;

        public ColorChange[] palette;

        static public void ClearPropertyBlock(SelectionGroup group)
        {
            foreach (var r in SelectionGroupUtility.GetComponents<Renderer>(group.name))
            {
                r.SetPropertyBlock(null);
            }
        }

        public void SetPropertyBlock(SelectionGroup group, float weight)
        {
            var renderers = SelectionGroupUtility.GetComponents<Renderer>(group.name).ToArray();
            foreach (var p in palette)
            {
                foreach (var r in renderers)
                {
                    if (Match(p.memberNameQuery, r.name))
                        Apply(r, weight);
                }
            }
        }

        void Apply(Renderer renderer, float weight)
        {
            var mpb = new MaterialPropertyBlock();
            //find existing colors
            var useExistingPropertyBlock = renderer.HasPropertyBlock();
            if (useExistingPropertyBlock)
            {
                renderer.GetPropertyBlock(mpb);
            }
            else
            {
                foreach (var cc in palette)
                {
                    if (renderer.sharedMaterial.HasProperty(cc.propertyName))
                    {
                        var color = renderer.sharedMaterial.GetColor(cc.propertyName);
                        mpb.SetColor(cc.propertyName, color);
                    }
                }
            }
            foreach (var cc in palette)
            {

                if (renderer.sharedMaterial.HasProperty(cc.propertyName))
                {
                    var colorA = renderer.sharedMaterial.GetColor(cc.propertyName);
                    mpb.SetColor(cc.propertyName, Color.Lerp(colorA, cc.color, weight));
                }
                else
                {
                    mpb.SetColor(cc.propertyName, cc.color);
                }
            }
            renderer.SetPropertyBlock(mpb);
        }


        bool Match(string query, string name)
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