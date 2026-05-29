using UnityEngine;

namespace AnimalMagicRoyale.Components.UI
{
    public class SpellInventoryUI : MonoBehaviour
    {
        [SerializeField] private SpellSlotUI[] slotUIs;
        [SerializeField] private SpellInventory trackedInventory;
        
        private void Update()
        {
            if (trackedInventory == null || slotUIs == null || slotUIs.Length == 0) return;
            
            for (int i = 0; i < slotUIs.Length && i < trackedInventory.slots.Length; i++)
            {
                var slot = trackedInventory.slots[i];
                bool isActive = (i == trackedInventory.activeSlotIndex);
                
                if (slotUIs[i] != null)
                {
                    slotUIs[i].UpdateSlot(slot, isActive);
                }
            }
        }
        
        public void SetTrackedInventory(SpellInventory inventory)
        {
            trackedInventory = inventory;
        }
    }
}
