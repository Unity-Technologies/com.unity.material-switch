using UnityEngine;
using UnityEngine.Playables;

namespace Unity.MaterialSwitch
{
    public class SpriteSwitchMixerPlayableBehaviour : PlayableBehaviour
    {

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
                    spriteGroup.spriteRenderers[i].sprite = spriteGroup.defaultSprites[i];
                }
                return;
            }

            var maxBehaviour = ((ScriptPlayable<SpriteSwitchPlayableBehaviour>) playable.GetInput(maxIndex)).GetBehaviour();
            foreach (var sr in spriteGroup.spriteRenderers)
            {
                sr.sprite = maxBehaviour.clip.sprite;
            }
        }
        
    }
}