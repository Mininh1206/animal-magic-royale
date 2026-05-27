using UnityEngine;
using AnimalMagicRoyale.Player;
using System.Collections;

namespace AnimalMagicRoyale.Components.Abilities
{
    [CreateAssetMenu(fileName = "FrenzyAbility", menuName = "Animal Magic Royale/Abilities/Frenzy")]
    public class FrenzyAbility : SpecialAbility
    {
        public float speedMultiplier = 1.5f;
        public float duration = 5f;

        public override void Activate(GameObject owner)
        {
            Debug.Log($"{owner.name} activated Frenzy! (+Speed and double damage)");
            var controller = owner.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.StartCoroutine(FrenzyRoutine(controller));
            }
        }

        private IEnumerator FrenzyRoutine(PlayerController controller)
        {
            float origMove = controller.moveSpeed;
            float origSprint = controller.sprintSpeed;

            controller.moveSpeed *= speedMultiplier;
            controller.sprintSpeed *= speedMultiplier;
            // Next attack double damage could be handled via a flag in PlayerController, e.g. HasDoubleDamage
            // For now, we apply the speed boost.

            yield return new WaitForSeconds(duration);

            controller.moveSpeed = origMove;
            controller.sprintSpeed = origSprint;
        }
    }
}
