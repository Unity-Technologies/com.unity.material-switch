using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.MaterialSwitch
{
    public class MaterialSlot
    {
        public Renderer renderer;
        public int index;
    }
    internal class MaterialPropertyBlockManager
    {
        public MaterialPropertyBlock block = new MaterialPropertyBlock();
        public Dictionary<string, RenderTexture> textures = new Dictionary<string, RenderTexture>();
        public Dictionary<string, Color> colors = new Dictionary<string, Color>();
        public Dictionary<string, float> floats = new Dictionary<string, float>();
        public Material material;
        
        
        public PalettePropertyMap globalMap;
        private Material textureLerpMaterial;

        private List<MaterialSlot> materialSlots = new List<MaterialSlot>();

        public void Clear()
        {
            foreach (var texture in textures.Values)
            {
                RenderTexture.active = texture;
                GL.Clear(true, true, Color.black);
                RenderTexture.active = null;
            }

            foreach (var c in colors.Keys)
            {
                colors[c] = Color.white;
            }
        }

        public void BlendPalettePropertyMap(float weight, PalettePropertyMap globalMap, PalettePropertyMap map)
        {
            BlendTextureProperties(weight, globalMap, map);
            BlendColorProperties(weight, globalMap, map);
            BlendFloatProperties(weight, globalMap, map);
        }

        private void BlendTextureProperties(float weight, PalettePropertyMap globalMap, PalettePropertyMap map)
        {
            if (textureLerpMaterial == null) textureLerpMaterial = CreateTextureLerpMaterial();
            var textureProperties = new Dictionary<string, TextureProperty>();
            foreach (var tp in map.textureProperties)
                if (tp.overrideBaseValue)
                    textureProperties[tp.propertyName] = tp;
            foreach (var tp in globalMap.textureProperties)
                if (tp.overrideBaseValue)
                    textureProperties[tp.propertyName] = tp;
            foreach (var kv in textureProperties)
            {
                var textureProperty = kv.Value;
                var texture = GetOrCreateFinalTexture(textureProperty);
                textureLerpMaterial.SetFloat("_Weight", weight);
                textureLerpMaterial.SetTexture("_TargetTex", textureProperty.targetValue);
                //Debug.Log($"Blend {textureProperty.propertyName} {textureProperty.targetValue} {weight}");
                var temp = RenderTexture.GetTemporary(texture.descriptor);
                Graphics.Blit(texture, temp, textureLerpMaterial);
                Graphics.Blit(temp, texture);
                RenderTexture.ReleaseTemporary(temp);
                block.SetTexture(textureProperty.propertyName, texture);
                // Debug.Log(texture);
            }
        }
        
        private void BlendColorProperties(float weight, PalettePropertyMap globalMap, PalettePropertyMap map)
        {
            var colorProperties = new Dictionary<string, ColorProperty>();
            foreach (var cp in map.colorCoordinates)
                if (cp.overrideBaseValue)
                    colorProperties[cp.propertyName] = cp;
            foreach (var cp in globalMap.colorCoordinates)
                if (cp.overrideBaseValue)
                    colorProperties[cp.propertyName] = cp;
            foreach (var kv in colorProperties)
            {
                var property = kv.Value;
                var color = GetOrCreateFinalColor(property);
                var newColor = color + property.targetValue * weight;
                //var color = Color.Lerp(i.baseValue, i.targetValue, weight);
                block.SetColor(property.propertyName,  newColor);
                colors[property.propertyName] = newColor;
            }
        }
        
        private void BlendFloatProperties(float weight, PalettePropertyMap globalMap, PalettePropertyMap map)
        {
            var floatProperties = new Dictionary<string, FloatProperty>();
            foreach (var cp in map.floatProperties)
                if (cp.overrideBaseValue)
                    floatProperties[cp.propertyName] = cp;
            foreach (var cp in globalMap.floatProperties)
                if (cp.overrideBaseValue)
                    floatProperties[cp.propertyName] = cp;
            foreach (var kv in floatProperties)
            {
                var property = kv.Value;
                var color = GetOrCreateFinalFloat(property);
                var newValue = color + property.targetValue * weight;
                //var color = Color.Lerp(i.baseValue, i.targetValue, weight);
                block.SetFloat(property.propertyName,  newValue);
                floats[property.propertyName] = newValue;
            }
        }

        private RenderTexture GetOrCreateFinalTexture(TextureProperty textureProperty)
        {
            if (!textures.TryGetValue(textureProperty.propertyName, out var texture))
            {
                texture = textures[textureProperty.propertyName] = new RenderTexture(
                    textureProperty.baseValue.width, textureProperty.baseValue.height, 0,
                    RenderTextureFormat.ARGB32);
                Graphics.Blit(textureProperty.baseValue, texture);
            }

            return texture;
        }
        
        private Color GetOrCreateFinalColor(ColorProperty property)
        {
            if (!colors.TryGetValue(property.propertyName, out var color))
            {
                color = colors[property.propertyName] = property.baseValue;
            }

            return color;
        }
        
        private float GetOrCreateFinalFloat(FloatProperty property)
        {
            if (!floats.TryGetValue(property.propertyName, out var f))
            {
                f  = floats[property.propertyName] = property.baseValue;
            }

            return f;
        }

        static void LerpCurrentColorsToTargetColors(float weight, PalettePropertyMap map,
            MaterialPropertyBlock block)
        {
            //lerp the colors towards targets.
            foreach (var i in map.colorCoordinates)
            {
                if (i.overrideBaseValue)
                {
                    var color = block.GetColor(i.propertyName);
                    //var color = Color.Lerp(i.baseValue, i.targetValue, weight);
                    block.SetColor(i.propertyName, color + i.targetValue * weight );
                }
            }
        }

        static void LerpCurrentFloatsToTargetFloats(float weight, PalettePropertyMap map, MaterialPropertyBlock block)
        {
            //lerp the floats towards targets.
            foreach (var i in map.floatProperties)
            {
                if (i.overrideBaseValue)
                {
                    var v = Mathf.Lerp(i.baseValue, i.targetValue, weight);
                    block.SetFloat(i.propertyName, v);
                }
            }
        }

        static Material CreateTextureLerpMaterial()
        {
            var m = new Material(Shader.Find("Hidden/TextureLerp"));
            return m;
        }

        public void ApplyMaterialPropertyBlockToRenderers()
        {
            foreach (var materialSlot in materialSlots)
            {
                // Debug.Log($"{materialSlot.renderer} {materialSlot.index}");
                materialSlot.renderer.SetPropertyBlock(block, materialSlot.index);
            }
        }
        public void AddRenderer(Renderer renderer, int index)
        {
            materialSlots.Add(new MaterialSlot()
            {
                renderer = renderer, index = index
            });
        }
    }
}