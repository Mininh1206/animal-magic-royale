using UnityEngine;
using UnityEngine.UIElements;

namespace AnimalMagicRoyale.Components.UI
{
    public class SpellInventoryUI : MonoBehaviour
    {
        private VisualElement[] slotUIs;
        private SpellInventory trackedInventory;
        
        public void Initialize(VisualElement root)
        {
            var container = root.Q<VisualElement>("spell-inventory-container");
            if (container != null)
            {
                // Buscamos hijos directos que sean spell slots, pero actualmente están vacíos en el UXML.
                // Como los vamos a generar o predefinir, asumiremos que los slots están como "spell-slot-0", "spell-slot-1", etc.
                slotUIs = new VisualElement[3]; // Max 3 slots
                for (int i = 0; i < 3; i++)
                {
                    slotUIs[i] = root.Q<VisualElement>($"spell-slot-{i}");
                }
            }
        }
        
        private void Update()
        {
            if (trackedInventory == null || slotUIs == null || slotUIs.Length == 0) return;
            
            for (int i = 0; i < slotUIs.Length && i < trackedInventory.slots.Length; i++)
            {
                var slotData = trackedInventory.slots[i];
                bool isActive = (i == trackedInventory.activeSlotIndex);
                
                var visualSlot = slotUIs[i];
                if (visualSlot != null)
                {
                    // Lógica de SpellSlotUI incorporada aquí para UI Toolkit
                    var icon = visualSlot.Q<VisualElement>("icon");
                    var bg = visualSlot.Q<VisualElement>("bg");
                    var cdOverlay = visualSlot.Q<VisualElement>("cooldown");
                    var keyText = visualSlot.Q<Label>("key");

                    if (icon != null)
                    {
                        if (slotData.IsEmpty || slotData.spellData == null)
                        {
                            icon.style.backgroundImage = null;
                        }
                        else
                        {
                            icon.style.backgroundImage = new StyleBackground(slotData.spellData.icon);
                        }
                    }

                    if (bg != null)
                    {
                        bg.style.borderTopColor = isActive ? Color.green : Color.clear;
                        bg.style.borderBottomColor = isActive ? Color.green : Color.clear;
                        bg.style.borderLeftColor = isActive ? Color.green : Color.clear;
                        bg.style.borderRightColor = isActive ? Color.green : Color.clear;
                    }

                    if (cdOverlay != null)
                    {
                        if (slotData.IsOnCooldown)
                        {
                            cdOverlay.style.display = DisplayStyle.Flex;
                            cdOverlay.style.height = Length.Percent(slotData.CooldownRemaining / slotData.spellData.cooldown * 100f);
                        }
                        else
                        {
                            cdOverlay.style.display = DisplayStyle.None;
                        }
                    }

                    if (keyText != null)
                    {
                        // TODO: Implementar lógica de teclas (Input System) si es necesario
                    }
                }
            }
        }
        
        public void SetTrackedInventory(SpellInventory inventory)
        {
            trackedInventory = inventory;
        }
    }
}
