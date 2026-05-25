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

        public void Awake()
        {
            CurrentHealth = maxHealth;
            IsAlive = true;
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0) return;

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
                    currentHealth = CurrentHealth,
                    maxHealth = maxHealth,
                    delta = -amount
                });
            }

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
            
            if (onDeath != null)
            {
                onDeath.Raise(gameObject);
            }
            
            gameObject.SetActive(false);
        }
    }
}
