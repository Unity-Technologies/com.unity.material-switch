using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


[CreateAssetMenu]
public class MaterialBlender : ScriptableObject
{
    public enum PropType
    {
        Color,
        Vector,
        Float,
        Range,
        Texture,
        Int,
    }
    
    [Serializable]
    public struct RuntimeMaterialProperty
    {
        public string name;
        public PropType type;
        public float floatValue;
        public int intValue;
        public Vector4 vectorValue;
        public Texture textureValue;
        public Color colorValue;

        public void Deconstruct(out string propertyname, out PropType type)
        {
            propertyname = this.name;
            type = this.type;
        }
    }

    public List<RuntimeMaterialProperty> propertyKeys = new List<RuntimeMaterialProperty>();
    
    public Material sourceMaterial;
    public Material targetMaterial;
    public Material blendedMaterial;
    [Range(0,1)]
    public float blend = 0;
    
    public void InterpolateMaterialProperties()
    {
        var blender = this;
        var mbp = new MaterialPropertyBlock();
        foreach (var pk in propertyKeys)
        {
            var sourceP = GetProperty(sourceMaterial, pk);
            var targetP = GetProperty(targetMaterial, pk);
            Interpolate(sourceP, targetP, mbp, blend);
        }

        ApplyMaterialPropertyBlock(mbp, blender.blendedMaterial);
    }

    private RuntimeMaterialProperty GetProperty(Material material, RuntimeMaterialProperty pk)
    {
        switch (pk.type)
        {
            case PropType.Color:
                pk.colorValue = material.GetColor(pk.name);
                break;
            case PropType.Vector:
                pk.vectorValue = material.GetVector(pk.name);
                break;
            case PropType.Float:
                pk.floatValue = material.GetFloat(pk.name);
                break;
            case PropType.Range:
                pk.floatValue = material.GetFloat(pk.name);
                break;
            case PropType.Texture:
                pk.textureValue = material.GetTexture(pk.name);
                break;
            case PropType.Int:
                pk.intValue = material.GetInt(pk.name);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return pk;
    }

    private void ApplyMaterialPropertyBlock(MaterialPropertyBlock mbp, Material mat)
    {
        foreach (var mp in this.propertyKeys)
        {
            switch (mp.type)
            {
                case PropType.Color:
                    mat.SetColor(mp.name, mbp.GetColor(mp.name));           
                    break;
                case PropType.Vector:
                    mat.SetVector(mp.name, mbp.GetVector(mp.name));
                    break;
                case PropType.Float:
                    mat.SetFloat(mp.name, mbp.GetFloat(mp.name));
                    break;
                case PropType.Range:
                    mat.SetFloat(mp.name, mbp.GetFloat(mp.name));
                    break;
                case PropType.Texture:
                    break;
                case PropType.Int:
                    mat.SetInt(mp.name, mbp.GetInt(mp.name));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void Interpolate(RuntimeMaterialProperty sourceP, RuntimeMaterialProperty targetP, MaterialPropertyBlock mbp, float t)
    {
        Assert.AreEqual(sourceP.type, targetP.type);
        Assert.AreEqual(sourceP.name, targetP.name);
        
        switch (sourceP.type)
        {
            case PropType.Color:
                mbp.SetColor(sourceP.name, Color.Lerp(sourceP.colorValue, targetP.colorValue, t));
                break;
            case PropType.Vector:
                mbp.SetVector(sourceP.name, Vector4.Lerp(sourceP.vectorValue, targetP.vectorValue, t));
                break;
            case PropType.Float:
                mbp.SetFloat(sourceP.name, Mathf.Lerp(sourceP.floatValue, targetP.floatValue, t));
                break;
            case PropType.Range:
                mbp.SetFloat(sourceP.name, Mathf.Lerp(sourceP.floatValue, targetP.floatValue, t));
                break;
            case PropType.Texture: break;
            case PropType.Int:
                mbp.SetInt(sourceP.name, Mathf.RoundToInt(Mathf.Lerp(sourceP.intValue, targetP.intValue, t)));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
