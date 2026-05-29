using UnityEngine;
using Blocks.Gameplay.Core;

namespace AnimalMagicRoyale.Core
{
    [System.Serializable]
    public struct HealthChangedPayload
    {
        public GameObject target;
        public GameObject source;
        public float currentHealth;
        public float maxHealth;
        public float delta;
    }

    [CreateAssetMenu(fileName = "HealthChangedEvent", menuName = "Animal Magic Royale/Events/Health Changed Event")]
    public class HealthChangedEvent : GameEvent<HealthChangedPayload>
    {
    }
}
