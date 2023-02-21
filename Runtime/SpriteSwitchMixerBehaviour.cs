using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.MaterialSwitch
{
    internal class SpriteSwitchMixerPlayableBehaviour : PlayableBehaviour
    {
        private Dictionary<SpriteRenderer, MaterialPropertyBlock> _propertyBlocks = new Dictionary<SpriteRenderer, MaterialPropertyBlock>();
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var group = playerData as SelectionGroups.SelectionGroup;
            if (group == null) return;
            if (!group.TryGetComponent(out SpriteGroup spriteGroup))
            {
                spriteGroup = group.gameObject.AddComponent<SpriteGroup>();
            }
            
            
            if (Application.isEditor && !Application.isPlaying)
            {
                spriteGroup.CollectSpriteRenderers();
            }
            
            var inputCount = playable.GetInputCount();

            var maxWeight = float.MinValue;
            var maxIndex = -1;
            //get total weight of all playables that are currently being mixed.
            var totalWeight = 0f;
            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                totalWeight += weight;
                if (weight > maxWeight)
                {
                    maxIndex = i;
                    maxWeight = weight;
                }
            }

            //weights should add up to 1.0, therefore calculate any missing weight using 1 - total.
            var missingWeight = 1f - totalWeight;
            //there is nothing to do (missing weight = 1 or total weight = 0)
            if (missingWeight >= 1f)
            {
                for (var i = 0; i < spriteGroup.spriteRenderers.Length; i++)
                {
                    var spriteRenderer = spriteGroup.spriteRenderers[i];
                    SetSpriteSheet(spriteRenderer, spriteRenderer.sprite.texture);
                }
                return;
            }

            var maxBehaviour = ((ScriptPlayable<SpriteSwitchPlayableBehaviour>) playable.GetInput(maxIndex)).GetBehaviour();
            
            for (var i = 0; i < spriteGroup.spriteRenderers.Length; i++)
            {
                var spriteRenderer = spriteGroup.spriteRenderers[i];
                SetSpriteSheet(spriteRenderer, maxBehaviour.clip.spriteSheet);
            }
        }

        private void SetSpriteSheet(SpriteRenderer spriteRenderer, Texture2D spriteSheet)
        {
            // This is required to override the animator controller which will also set the
            // property block on this sprite renderer.
            SpriteSwitchMonoBehaviour.OnLateUpdate(() =>
            {
                if (spriteSheet == null)
                {
                    spriteRenderer.SetPropertyBlock(null);
                    return;
                }

                if (!_propertyBlocks.TryGetValue(spriteRenderer, out var propertyBlock))
                    propertyBlock = new MaterialPropertyBlock();

                spriteRenderer.GetPropertyBlock(propertyBlock);
                _propertyBlocks[spriteRenderer] = propertyBlock;
                propertyBlock.SetTexture(MainTex, spriteSheet);
                spriteRenderer.SetPropertyBlock(propertyBlock);
            });
        }
    }
}