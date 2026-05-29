using UnityEngine;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Components
{
    public class HealthComponent : MonoBehaviour
    {
        [Header("Settings")]
        public float maxHealth = 100f;
        
        [Header("Events")]
        public HealthChangedEvent onHealthChanged;
        public DeathEvent onDeath;

        public float CurrentHealth { get; private set; }
        public bool IsAlive { get; private set; }
        private GameObject lastDamager;

        public void Awake()
        {
            CurrentHealth = maxHealth;
            IsAlive = true;
            lastDamager = null;
        }

        public void TakeDamage(float amount, GameObject source = null)
        {
            if (!IsAlive || amount <= 0) return;

            // Prevent damage if the game is not in Playing state
            if (AnimalMagicRoyale.Core.GameManager.Instance != null && 
                !(AnimalMagicRoyale.Core.GameManager.Instance.StateMachine.CurrentState is AnimalMagicRoyale.Core.PlayingState))
            {
                return;
            }

            if (source != null)
            {
                lastDamager = source;
            }

            var shield = GetComponent<AnimalMagicRoyale.Components.Abilities.ShieldComponent>();
            if (shield != null && shield.remainingShield > 0)
            {
                float absorbed = Mathf.Min(amount, shield.remainingShield);
                shield.remainingShield -= absorbed;
                amount -= absorbed;
                if (amount <= 0) return; // Completely absorbed
            }

            CurrentHealth -= amount;
            
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
            }

            if (onHealthChanged != null)
            {
                onHealthChanged.Raise(new HealthChangedPayload
                {
                    target = gameObject,
                    source = source,
                    currentHealth = CurrentHealth,
                    maxHealth = maxHealth,
                    delta = -amount
                });
            }

            Debug.Log($"[HealthComponent] {gameObject.name} took {amount} dmg from {source?.name ?? "environment"}. HP: {CurrentHealth}/{maxHealth}");

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0) return;

            CurrentHealth += amount;
            if (CurrentHealth > maxHealth)
            {
                CurrentHealth = maxHealth;
            }

            if (onHealthChanged != null)
            {
                onHealthChanged.Raise(new HealthChangedPayload
                {
                    target = gameObject,
                    source = null,
                    currentHealth = CurrentHealth,
                    maxHealth = maxHealth,
                    delta = amount
                });
            }
        }

        private void Die()
        {
            if (!IsAlive) return;
            
            IsAlive = false;
            Debug.Log($"[HealthComponent] {gameObject.name} has been killed by {lastDamager?.name ?? "environment"}");
            
            if (onDeath != null)
            {
                onDeath.Raise(new DeathPayload
                {
                    victim = gameObject,
                    killer = lastDamager
                });
            }
            
            gameObject.SetActive(false);
        }
    }
}
