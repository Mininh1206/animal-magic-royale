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
            if (onHealthChanged == null) onHealthChanged = Resources.Load<HealthChangedEvent>("Events/HealthChangedEvent");
            if (onDeath == null) onDeath = Resources.Load<DeathEvent>("Events/DeathEvent");

            CurrentHealth = maxHealth;
            IsAlive = true;
            lastDamager = null;
        }

        public void TakeDamage(float amount, GameObject source = null)
        {
            if (!IsAlive || amount <= 0) return;

            // Fuego amigo protection: don't damage teammates
            if (source != null && AnimalMagicRoyale.Core.TeamManager.Instance != null)
            {
                if (AnimalMagicRoyale.Core.TeamManager.Instance.AreTeammates(gameObject, source))
                {
                    return; // Ignorar daño de aliados
                }
            }

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
                Debug.Log($"[HealthComponent] {gameObject.name}: Raising DeathEvent (asset: {onDeath.name})");
                onDeath.Raise(new DeathPayload
                {
                    victim = gameObject,
                    killer = lastDamager
                });
            }
            else
            {
                Debug.LogWarning($"[HealthComponent] {gameObject.name}: onDeath event is NULL! KillFeed chain will not fire via events.");
            }

            // Fallback directo robusto: si el GameManager existe, le notificamos directamente
            // para evitar problemas de configuración del Inspector con los ScriptableObjects.
            if (AnimalMagicRoyale.Core.GameManager.Instance != null)
            {
                Debug.Log($"[HealthComponent] Fallback: Notificando directamente al GameManager de la muerte de {gameObject.name}");
                AnimalMagicRoyale.Core.GameManager.Instance.UnregisterPlayer(gameObject, lastDamager);
            }
            
            // Drop all spells
            var inventory = GetComponent<SpellInventory>();
            if (inventory != null)
            {
                for (int i = 0; i < inventory.slots.Length; i++)
                {
                    if (!inventory.slots[i].IsEmpty)
                    {
                        inventory.DropSpell(inventory.slots[i].spellData);
                    }
                }
            }

            gameObject.SetActive(false);
        }
    }
}
