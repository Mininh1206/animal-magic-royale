using UnityEngine;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Components
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(SpellInventory))]
    [RequireComponent(typeof(CharacterAnimationHandler))]
    [RequireComponent(typeof(ZoneDamageTracker))]
    public abstract class BasicController : MonoBehaviour
    {
        public HealthComponent Health { get; protected set; }
        public SpellInventory Inventory { get; protected set; }
        public CharacterAnimationHandler AnimHandler { get; protected set; }

        protected virtual void Awake()
        {
            Health = GetComponent<HealthComponent>();
            Inventory = GetComponent<SpellInventory>();
            AnimHandler = GetComponent<CharacterAnimationHandler>();
        }

        protected virtual void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterPlayer(gameObject);
            }
        }

        public void TakeDamage(float amount, GameObject source = null)
        {
            Health.TakeDamage(amount, source);
        }

        public void Heal(float amount)
        {
            Health.Heal(amount);
        }
    }
}
