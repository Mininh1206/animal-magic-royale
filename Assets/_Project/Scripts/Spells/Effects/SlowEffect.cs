using UnityEngine;
using AnimalMagicRoyale.Player;
using System.Collections;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "SlowEffect", menuName = "Animal Magic Royale/Spells/Effects/Slow")]
    public class SlowEffect : SpellEffect
    {
        public float slowPercent;
        public float duration;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            
            var playerController = target.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.StartCoroutine(ApplySlowRoutine(playerController));
            }
            else
            {
                var botController = target.GetComponent<AnimalMagicRoyale.AI.BotController>();
                if (botController != null)
                {
                    botController.StartCoroutine(ApplySlowBotRoutine(botController));
                }
            }
        }

        private IEnumerator ApplySlowBotRoutine(AnimalMagicRoyale.AI.BotController controller)
        {
            // Debug.Log($"[SlowEffect] Slow aplicado al bot {controller.gameObject.name} por {duration}s.");
            float originalSpeed = controller.Agent.speed;
            controller.Agent.speed *= (1f - slowPercent);

            yield return new WaitForSeconds(duration);

            if (controller != null && controller.Agent != null)
            {
                controller.Agent.speed = originalSpeed;
            }
        }

        private IEnumerator ApplySlowRoutine(PlayerController controller)
        {
            float originalMoveSpeed = controller.moveSpeed;
            float originalSprintSpeed = controller.sprintSpeed;

            controller.moveSpeed *= (1f - slowPercent);
            controller.sprintSpeed *= (1f - slowPercent);

            yield return new WaitForSeconds(duration);

            controller.moveSpeed = originalMoveSpeed;
            controller.sprintSpeed = originalSprintSpeed;
        }
    }
}
