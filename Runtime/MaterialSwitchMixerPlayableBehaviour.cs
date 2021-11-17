using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.MaterialSwitch
{
    internal class MaterialSwitchMixerPlayableBehaviour : PlayableBehaviour
    {
        Material textureLerpMaterial;

        HashSet<Renderer> renderers;

        HashSet<MaterialProperties> activePalettePropertyMapInstances= new HashSet<MaterialProperties>();

        private Dictionary<Material, MaterialPropertyBlockManager> blockManagers =
            new Dictionary<Material, MaterialPropertyBlockManager>();

        public override void OnPlayableDestroy(Playable playable)
        {
            RemoveMaterialPropertyBlocks();
            foreach(var ppm in activePalettePropertyMapInstances) {
                foreach(var i in ppm.textureProperties) {
                    if(i.finalTexture != null) {
                        i.finalTexture.Release();
                        i.finalTexture = null;
                    }
                }
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var group = playerData as SelectionGroups.Runtime.SelectionGroup;
            if (group == null) return;
            var materialGroup = group.GetComponent<MaterialGroup>();
            if (materialGroup == null) return;
            if (Application.isEditor && !Application.isPlaying)
            {
                materialGroup.CollectMaterials();
                renderers = null;
                blockManagers = null;
            }

            //a group has many renderers, get them all. This will not change over the duration of the track
            if (renderers == null) renderers = new HashSet<Renderer>(group.GetMemberComponents<Renderer>());
            if (blockManagers == null) blockManagers = new Dictionary<Material, MaterialPropertyBlockManager>();
            


            var inputCount = playable.GetInputCount();

            //get total weight of all playables that are currently being mixed.
            var totalWeight = 0f;
            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                totalWeight += weight;
            }

            //weights should add up to 1.0, therefore calculate any missing weight using 1 - total.
            var missingWeight = 1f - totalWeight;
            //there is nothing to do (missing weight = 1 or total weight = 0) remove any property blocks then exit.
            if (missingWeight >= 1f)
            {
                RemoveMaterialPropertyBlocks();
                return;
            }
            CreateBlockManagers();
            UpdateBlockManagers(playable);
            ApplyBlockManagers();

        }

        private void ApplyBlockManagers()
        {
            foreach (var blockManager in blockManagers.Values)
            {
                blockManager.ApplyMaterialPropertyBlockToRenderers();
            }
        }

        void CreateBlockManagers()
        {
            foreach (var renderer in renderers)
            {
                for (var index = 0; index < renderer.sharedMaterials.Length; index++)
                {
                    var material = renderer.sharedMaterials[index];
                    if (!blockManagers.TryGetValue(material, out var bm))
                    {
                        bm = blockManagers[material] = new MaterialPropertyBlockManager();    
                    }
                    bm.material = material;
                    bm.AddRenderer(renderer, index);
                }
            }
        }

        private void UpdateBlockManagers(Playable playable)
        {

            var inputCount = playable.GetInputCount();
            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                if (weight == 0) continue;

                var behaviour = ((ScriptPlayable<MaterialSwitchPlayableBehaviour>) playable.GetInput(i)).GetBehaviour();
                
                foreach (var pm in behaviour.materialPropertiesList)
                {
                    if (blockManagers.TryGetValue(pm.material, out var bm))
                    {
                        bm.BlendPalettePropertyMap(weight, behaviour.clip.globalMaterialProperties, pm);
                    }
                }
            }
        }

        

        void RemoveMaterialPropertyBlocks()
        {
            if (renderers != null)
                foreach (var r in renderers)
                {
                    if (r == null) continue;
                    for (var i = 0; i < r.sharedMaterials.Length; i++)
                    {
                        r.SetPropertyBlock(null, i);
                    }
                }
        }
    }
}