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

            if (Vector3.Distance(transform.position, player.transform.position) <= interactionRange)
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
            
            var inventory = player.GetComponent<SpellInventory>();
            if (inventory != null && containedSpell != null)
            {
                inventory.TryPickupSpell(containedSpell);
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
