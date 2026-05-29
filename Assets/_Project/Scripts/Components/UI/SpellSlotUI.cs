using UnityEngine;
using UnityEngine.UI;
using AnimalMagicRoyale.Spells;
using AnimalMagicRoyale.Core;
using TMPro;

namespace AnimalMagicRoyale.Components.UI
{
    public class SpellSlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay; // Image component with ImageType=Filled
        [SerializeField] private GameObject activeIndicator;
        [SerializeField] private GameObject emptyIndicator;
        [SerializeField] private TextMeshProUGUI keyText;
        [SerializeField] private KeyBindingManager.GameAction slotAction;
        
        private void Awake()
        {
            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
            }
        }
        
        public void UpdateSlot(SpellSlot slot, bool isActive)
        {
            if (activeIndicator != null)
                activeIndicator.SetActive(isActive);
                
            if (keyText != null && KeyBindingManager.Instance != null)
            {
                string keyName = KeyBindingManager.Instance.GetBinding(slotAction).ToString();
                keyText.text = keyName.Replace("Digit", "");
                keyText.color = isActive ? Color.green : Color.white;
            }
                
            if (slot.IsEmpty)
            {
                if (emptyIndicator != null) emptyIndicator.SetActive(true);
                if (iconImage != null) iconImage.enabled = false;
                if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
            }
            else
            {
                if (emptyIndicator != null) emptyIndicator.SetActive(false);
                
                if (iconImage != null)
                {
                    iconImage.enabled = true;
                    if (iconImage.sprite != slot.spellData.icon)
                    {
                        iconImage.sprite = slot.spellData.icon;
                    }
                }
                
                if (cooldownOverlay != null)
                {
                    if (slot.IsOnCooldown)
                    {
                        float remaining = slot.CooldownRemaining;
                        cooldownOverlay.fillAmount = remaining / slot.spellData.cooldown;
                    }
                    else
                    {
                        cooldownOverlay.fillAmount = 0f;
                    }
                }
            }
        }
    }
}
