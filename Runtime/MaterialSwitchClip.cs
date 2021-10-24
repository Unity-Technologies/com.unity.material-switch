﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.MaterialSwitch
{

    internal class MaterialSwitchClip : PlayableAsset
    {
        public PalettePropertyMap globalPalettePropertyMap;
        public List<PalettePropertyMap> palettePropertyMap;

        [NonSerialized] public Dictionary<Material, PalettePropertyMap> materialMap = null;

        void OnValidate()
        {
            foreach (var ppm in palettePropertyMap)
            {
                if (ppm.texture == null) continue;
                if(!ppm.texture.isReadable) continue;
                for (int i = 0; i < ppm.colorCoordinates.Count; i++)
                {
                    var cc = ppm.colorCoordinates[i];
                    cc.targetValue = ppm.texture.GetPixel((int)cc.uv.x, (int)cc.uv.y);
                    ppm.colorCoordinates[i] = cc;
                }
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<MaterialSwitchPlayableBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.clip = this;
            behaviour.palettePropertyMap = palettePropertyMap;
            return playable;
        }
    }
}