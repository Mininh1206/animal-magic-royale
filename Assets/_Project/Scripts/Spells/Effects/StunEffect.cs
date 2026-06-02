using UnityEngine;
using AnimalMagicRoyale.Player;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "StunEffect", menuName = "Animal Magic Royale/Spells/Effects/Stun")]
    public class StunEffect : SpellEffect
    {
        public float duration;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            
            var playerController = target.GetComponent<PlayerController>();
            if (playerController != null && playerController.StunnedState != null)
            {
                playerController.StunnedState.SetDuration(duration);
                playerController.StateMachine.ChangeState(playerController.StunnedState);
            }
            else
            {
                var botController = target.GetComponent<AnimalMagicRoyale.AI.BotController>();
                if (botController != null)
                {
                    botController.StartCoroutine(ApplyStunBotRoutine(botController));
                }
            }
        }

        private System.Collections.IEnumerator ApplyStunBotRoutine(AnimalMagicRoyale.AI.BotController botController)
        {
            Debug.Log($"[StunEffect] Stun aplicado al bot {botController.gameObject.name} por {duration}s.");
            if (botController.Agent != null && botController.Agent.isOnNavMesh)
            {
                botController.Agent.isStopped = true;
                botController.Agent.ResetPath();
            }

            yield return new WaitForSeconds(duration);

            if (botController != null && botController.Agent != null && botController.Agent.isOnNavMesh)
            {
                botController.Agent.isStopped = false;
            }
        }
    }
}
