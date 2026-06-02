using UnityEngine;
using AnimalMagicRoyale.Spells;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Components
{
    public class LootBox : MonoBehaviour
    {
        [SerializeField] private SpellTier tier;
        [SerializeField] private float interactionRange = 2f;


        private bool isOpened = false;
        private SpellData containedSpell;
        public LootBoxSpawner Spawner { get; set; }
        public Transform SpawnPoint { get; set; }

        public void Initialize(SpellData spell, SpellTier tier)
        {
            this.containedSpell = spell;
            this.tier = tier;
            isOpened = false;
            gameObject.SetActive(true);
        }

        public bool TryOpen(GameObject player)
        {
            if (isOpened) return false;

            float effectiveRange = Mathf.Max(interactionRange, 3f);
            if (Vector3.Distance(transform.position, player.transform.position) <= effectiveRange)
            {
                // In a real scenario with hold interaction, we'd check input over time here.
                // Since user approved interaction, we assume the input system calls this when E is pressed.
                OnOpened(player);
                return true;
            }
            return false;
        }

        private void OnOpened(GameObject player)
        {
            isOpened = true;
            
            if (containedSpell != null)
            {
                // Drop the spell instead of adding directly
                Vector3 dropPosition = transform.position + Vector3.up * 0.1f;
                GameObject dropGO = new GameObject($"Dropped_{containedSpell.spellName}");
                dropGO.transform.position = dropPosition;
                
                var pickup = dropGO.AddComponent<SpellPickup>();
                pickup.Initialize(containedSpell);
            }

            // Could emit event here if we had a reference to LootBoxOpenedEvent, 
            // usually done via a central EventManager or direct reference.
            
            if (Spawner != null)
            {
                Spawner.OnBoxOpened(this);
            }

            gameObject.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
