using UnityEngine;
using UnityEngine.UIElements;

namespace AnimalMagicRoyale.Components.UI
{
    public class SpellInventoryUI : MonoBehaviour
    {
        private VisualElement[] slotUIs;
        private Label activeSpellNameLabel;
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

            activeSpellNameLabel = root.Q<Label>("active-spell-name");
        }
        
        private void Update()
        {
            if (trackedInventory == null || slotUIs == null || slotUIs.Length == 0) return;
            
            for (int i = 0; i < slotUIs.Length && i < trackedInventory.slots.Length; i++)
            {
                var slotData = trackedInventory.slots[i];
                bool isActive = (i == trackedInventory.activeSlotIndex);
                
                if (isActive && activeSpellNameLabel != null)
                {
                    if (!slotData.IsEmpty && slotData.spellData != null)
                    {
                        activeSpellNameLabel.text = slotData.spellData.spellName;
                        activeSpellNameLabel.style.color = slotData.spellData.spellColor;
                    }
                    else
                    {
                        activeSpellNameLabel.text = "VACÍO";
                        activeSpellNameLabel.style.color = Color.gray;
                    }
                }

                var visualSlot = slotUIs[i];
                if (visualSlot != null)
                {
                    // Agrandar el slot activo para mayor feedback visual
                    visualSlot.style.scale = isActive ? new StyleScale(new Scale(new Vector2(1.2f, 1.2f))) : new StyleScale(new Scale(Vector2.one));

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
                            icon.style.unityBackgroundScaleMode = ScaleMode.StretchToFill;
                            
                            // Aseguramos de que el VisualElement icono ocupe el 100% de su contenedor
                            icon.style.width = Length.Percent(100);
                            icon.style.height = Length.Percent(100);
                        }
                    }

                    if (bg != null)
                    {
                        // Aseguramos que tenga grosor de borde para que se vea el color
                        bg.style.borderTopWidth = 4;
                        bg.style.borderBottomWidth = 4;
                        bg.style.borderLeftWidth = 4;
                        bg.style.borderRightWidth = 4;

                        // Redondear los bordes
                        bg.style.borderTopLeftRadius = 8;
                        bg.style.borderTopRightRadius = 8;
                        bg.style.borderBottomLeftRadius = 8;
                        bg.style.borderBottomRightRadius = 8;

                        Color activeBorderColor = new Color(0.2f, 1f, 0.2f, 1f); // Verde brillante
                        bg.style.borderTopColor = isActive ? activeBorderColor : Color.clear;
                        bg.style.borderBottomColor = isActive ? activeBorderColor : Color.clear;
                        bg.style.borderLeftColor = isActive ? activeBorderColor : Color.clear;
                        bg.style.borderRightColor = isActive ? activeBorderColor : Color.clear;
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
