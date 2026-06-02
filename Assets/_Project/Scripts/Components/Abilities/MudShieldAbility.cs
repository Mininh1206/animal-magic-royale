using UnityEngine;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Components.Abilities
{
    [CreateAssetMenu(fileName = "MudShieldAbility", menuName = "Animal Magic Royale/Abilities/Mud Shield")]
    public class MudShieldAbility : SpecialAbility
    {
        public float shieldAmount = 50f;
        public float duration = 5f;

        public override void Activate(GameObject owner)
        {
            var shield = owner.AddComponent<ShieldComponent>();
            shield.Initialize(shieldAmount, duration);
            // Debug.Log($"{owner.name} activated Mud Shield! ({shieldAmount} HP for {duration}s)");
        }
    }

    public class ShieldComponent : MonoBehaviour
    {
        public float remainingShield;
        private float expireTime;

        public void Initialize(float amount, float duration)
        {
            remainingShield = amount;
            expireTime = Time.time + duration;
        }

        private void Update()
        {
            if (Time.time >= expireTime)
            {
                Destroy(this);
            }
        }
    }
}
