using UnityEngine;
using AnimalMagicRoyale.Player;
using System.Collections;

namespace AnimalMagicRoyale.Components.Abilities
{
    [CreateAssetMenu(fileName = "EmergencyFlightAbility", menuName = "Animal Magic Royale/Abilities/Emergency Flight")]
    public class EmergencyFlightAbility : SpecialAbility
    {
        public float jumpImpulse = 15f;
        public float glideGravity = -5f;
        public float duration = 4f;

        public override void Activate(GameObject owner)
        {
            Debug.Log($"{owner.name} activated Emergency Flight!");
            var controller = owner.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.StartCoroutine(FlightRoutine(controller));
            }
        }

        private IEnumerator FlightRoutine(PlayerController controller)
        {
            controller.VerticalVelocity = jumpImpulse;
            float origGravity = controller.gravity;
            controller.gravity = glideGravity;

            yield return new WaitForSeconds(duration);

            controller.gravity = origGravity;
        }
    }
}
