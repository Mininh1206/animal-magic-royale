using UnityEngine;
using UnityEngine.UIElements;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Spells;

namespace AnimalMagicRoyale.Components.UI
{
    public class InventoryUI : MonoBehaviour
    {
        public static InventoryUI Instance { get; private set; }

        private VisualElement rootContainer;
        private VisualElement inventoryRoot;
        
        private VisualElement[] slots = new VisualElement[3];
        private VisualElement[] icons = new VisualElement[3];
        
        private Label detailName;
        private Label detailTier;
        private Label detailDesc;
        private Label detailEffects;

        public bool IsOpen => inventoryRoot != null && inventoryRoot.style.display == DisplayStyle.Flex;

        private SpellInventory trackedInventory;
        
        // Drag and drop state
        private bool isDragging = false;
        private int draggedSlotIndex = -1;
        private VisualElement ghostIcon;
        private Vector2 dragStartPos;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        public void Initialize(VisualElement root)
        {
            rootContainer = root;
            inventoryRoot = root.Q<VisualElement>("inventory-root");
            
            if (inventoryRoot != null)
            {
                inventoryRoot.style.display = DisplayStyle.None;

                for (int i = 0; i < 3; i++)
                {
                    int index = i; // local capture
                    slots[i] = inventoryRoot.Q<VisualElement>($"inv-slot-{i}");
                    icons[i] = inventoryRoot.Q<VisualElement>($"inv-icon-{i}");
                    
                    if (slots[i] != null)
                    {
                        slots[i].RegisterCallback<PointerDownEvent>(evt => OnPointerDown(evt, index));
                        slots[i].RegisterCallback<PointerMoveEvent>(OnPointerMove);
                        slots[i].RegisterCallback<PointerUpEvent>(evt => OnPointerUp(evt, index));
                        slots[i].RegisterCallback<PointerEnterEvent>(evt => OnPointerEnter(evt, index));
                    }
                }

                detailName = inventoryRoot.Q<Label>("detail-name");
                detailTier = inventoryRoot.Q<Label>("detail-tier");
                detailDesc = inventoryRoot.Q<Label>("detail-desc");
                detailEffects = inventoryRoot.Q<Label>("detail-effects");

                // Ghost icon for drag and drop
                ghostIcon = new VisualElement();
                ghostIcon.style.position = Position.Absolute;
                ghostIcon.style.width = 80;
                ghostIcon.style.height = 80;
                ghostIcon.style.display = DisplayStyle.None;
                root.Add(ghostIcon);
            }
        }

        public void Toggle()
        {
            // Debug.Log($"[InventoryUI] Toggle called. Current state: IsOpen={IsOpen}");
            if (IsOpen) Hide();
            else Show();
        }

        public void Show()
        {
            // Debug.Log("[InventoryUI] Show inventory.");
            if (inventoryRoot == null) return;
            inventoryRoot.style.display = DisplayStyle.Flex;
            RefreshUI();
            
            // Show first non-empty spell details
            ShowDetailsForFirstAvailable();

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        public void Hide()
        {
            // Debug.Log("[InventoryUI] Hide inventory.");
            if (inventoryRoot == null) return;
            inventoryRoot.style.display = DisplayStyle.None;
            
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }

        public void SetTrackedInventory(SpellInventory inventory)
        {
            // Debug.Log($"[InventoryUI] Tracking new inventory.");
            trackedInventory = inventory;
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (trackedInventory == null) return;

            for (int i = 0; i < 3; i++)
            {
                var spellData = trackedInventory.slots[i].spellData;
                if (spellData != null && icons[i] != null)
                {
                    icons[i].style.backgroundImage = new StyleBackground(spellData.icon);
                }
                else if (icons[i] != null)
                {
                    icons[i].style.backgroundImage = null;
                }
            }
        }

        private void ShowDetailsForFirstAvailable()
        {
            if (trackedInventory == null) return;
            for (int i = 0; i < 3; i++)
            {
                if (trackedInventory.slots[i].spellData != null)
                {
                    ShowDetails(trackedInventory.slots[i].spellData);
                    return;
                }
            }
            ClearDetails();
        }

        private void ShowDetails(SpellData spellData)
        {
            if (spellData == null)
            {
                ClearDetails();
                return;
            }

            detailName.text = spellData.spellName;
            detailName.style.color = spellData.spellColor;
            
            detailTier.text = $"Tier: {spellData.tier}";
            detailTier.style.color = spellData.spellColor;
            
            detailDesc.text = spellData.description;

            string effectsStr = "";
            if (spellData.effects != null)
            {
                foreach(var effect in spellData.effects)
                {
                    if (effect is AnimalMagicRoyale.Spells.Effects.DamageEffect dmg) effectsStr += $"Daño: {dmg.damageAmount}\n";
                    else if (effect is AnimalMagicRoyale.Spells.Effects.SlowEffect slow) effectsStr += $"Ralentiza: {slow.slowPercent * 100}% ({slow.duration}s)\n";
                    else if (effect is AnimalMagicRoyale.Spells.Effects.HealEffect heal) effectsStr += $"Cura: {heal.healAmount}\n";
                    else if (effect is AnimalMagicRoyale.Spells.Effects.StunEffect stun) effectsStr += $"Stun: {stun.duration}s\n";
                    else if (effect is AnimalMagicRoyale.Spells.Effects.KnockbackEffect kb) effectsStr += $"Empuje: {kb.force}\n";
                }
            }
            detailEffects.text = effectsStr;
        }

        private void ClearDetails()
        {
            if (detailName == null) return;
            detailName.text = "Selecciona un hechizo";
            detailName.style.color = Color.white;
            detailTier.text = "";
            detailDesc.text = "";
            detailEffects.text = "";
        }

        private void OnPointerEnter(PointerEnterEvent evt, int index)
        {
            if (!isDragging && trackedInventory != null && trackedInventory.slots[index].spellData != null)
            {
                ShowDetails(trackedInventory.slots[index].spellData);
            }
        }

        private void OnPointerDown(PointerDownEvent evt, int index)
        {
            if (trackedInventory == null || trackedInventory.slots[index].spellData == null) return;

            isDragging = true;
            draggedSlotIndex = index;
            dragStartPos = evt.position;

            slots[index].CapturePointer(evt.pointerId);

            ghostIcon.style.display = DisplayStyle.Flex;
            ghostIcon.style.backgroundImage = new StyleBackground(trackedInventory.slots[index].spellData.icon);
            ghostIcon.style.left = evt.position.x - 40;
            ghostIcon.style.top = evt.position.y - 40;

            icons[index].style.opacity = 0.3f;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!isDragging) return;

            ghostIcon.style.left = evt.position.x - 40;
            ghostIcon.style.top = evt.position.y - 40;
        }

        private void OnPointerUp(PointerUpEvent evt, int index)
        {
            if (!isDragging || draggedSlotIndex != index) return;

            slots[index].ReleasePointer(evt.pointerId);
            isDragging = false;
            ghostIcon.style.display = DisplayStyle.None;
            icons[index].style.opacity = 1f;

            // Find target slot
            int targetIndex = -1;
            for (int i = 0; i < 3; i++)
            {
                if (slots[i].worldBound.Contains(evt.position))
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex != -1 && targetIndex != draggedSlotIndex)
            {
                SwapSpells(draggedSlotIndex, targetIndex);
            }

            draggedSlotIndex = -1;
        }

        private void SwapSpells(int indexA, int indexB)
        {
            if (trackedInventory == null) return;

            // Debug.Log($"[InventoryUI] Swapping spells between slot {indexA} and {indexB}");

            trackedInventory.SwapSlots(indexA, indexB);

            RefreshUI();
        }
    }
}
