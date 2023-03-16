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
            
            // This code is disabled until we are sure it is not required.
            // if (Application.isEditor && !Application.isPlaying)
            // {
            //     spriteGroup.CollectSpriteRenderers();
            // }
            
            var inputCount = playable.GetInputCount();
            
            for (var i = 0; i < inputCount; i++)
            {
                var weight = playable.GetInputWeight(i);
                // the first input on the track with a weight greater than 0 will be the only clip processed, as we do not mix clips.
                if (weight > 0)
                {
                    var behaviour = ((ScriptPlayable<SpriteSwitchPlayableBehaviour>) playable.GetInput(i)).GetBehaviour();
                    for (var j = 0; j < spriteGroup.spriteRenderers.Length; j++)
                    {
                        var spriteRenderer = spriteGroup.spriteRenderers[j];
                        SetSpriteSheet(spriteRenderer, behaviour.clip.spriteSheet);
                    }
                    return;
                }
            }
            
            for (var j = 0; j < spriteGroup.spriteRenderers.Length; j++)
            {
                var spriteRenderer = spriteGroup.spriteRenderers[j];
                SetSpriteSheet(spriteRenderer, spriteRenderer.sprite.texture);
            }
        }

        private void SetSpriteSheet(SpriteRenderer spriteRenderer, Texture2D spriteSheet)
        {
            // This is required to override the animator controller which will also set the
            // property block on this sprite renderer.
            SpriteSwitchEventInvoker.OnLateUpdate(() =>
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