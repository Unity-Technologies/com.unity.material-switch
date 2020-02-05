using System;
using System.Collections.Generic;
using Unity.SelectionGroups;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.PaletteSwitch
{
    [System.Serializable]
    public struct ColorCoordinate
    {
        public Vector2 uv;
        public string propertyName;
        public int propertyId;
        public Color sampledColor;
        public Color originalColor;
    }

    [System.Serializable]
    public class PalettePropertyMap
    {
        public Texture2D texture;
        public List<ColorCoordinate> colorCoordinates = new List<ColorCoordinate>();
        public Material material;
        public MaterialPropertyBlock materialPropertyBlock;
        public bool showCoords = false;
    }

    public class PaletteSwitchClip : PlayableAsset
    {
        public PalettePropertyMap[] palettePropertyMap;
        
        void OnValidate()
        {
            foreach(var ppm in palettePropertyMap) {
                for (int i = 0; i < ppm.colorCoordinates.Count; i++) {
                    var cc = ppm.colorCoordinates[i];
                    cc.sampledColor = ppm.texture.GetPixel((int)cc.uv.x, (int)cc.uv.y);
                    ppm.colorCoordinates[i] = cc;
                }
            }
        }
        
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<PaletteSwitchBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.palettePropertyMap = palettePropertyMap;
            return playable;
        }
    }
}