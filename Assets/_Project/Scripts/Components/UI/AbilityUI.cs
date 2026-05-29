using UnityEngine;
using UnityEngine.UI;
using AnimalMagicRoyale.Core;
using TMPro;

namespace AnimalMagicRoyale.Components.UI
{
    public class AbilityUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay; // Filled image for radial cooldown
        [SerializeField] private AbilityHolder trackedAbility;
        [SerializeField] private SpecialAbility defaultIconSource; // Optional
        [SerializeField] private TextMeshProUGUI keyText;
        [SerializeField] private KeyBindingManager.GameAction slotAction = KeyBindingManager.GameAction.Ability;
        
        private void Awake()
        {
            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
            }
        }

        private void Start()
        {
            if (iconImage != null && defaultIconSource != null && trackedAbility == null)
            {
                iconImage.sprite = defaultIconSource.icon;
            }
            else if (iconImage != null && trackedAbility != null)
            {
                iconImage.sprite = trackedAbility.Ability.icon;
            }
        }

        private void Update()
        {
            if (keyText != null && KeyBindingManager.Instance != null)
            {
                string keyName = KeyBindingManager.Instance.GetBinding(slotAction).ToString();
                keyText.text = keyName.Replace("Digit", "");
                // Mostrar en verde si está lista, blanco si no (o viceversa)
                keyText.color = (trackedAbility != null && trackedAbility.IsReady) ? Color.green : Color.white;
            }

            if (trackedAbility == null) return;
            
            if (cooldownOverlay != null)
            {
                if (!trackedAbility.IsReady)
                {
                    float remaining = trackedAbility.GetCooldownRemaining();
                    float total = trackedAbility.TotalCooldown;
                    if (total > 0f)
                    {
                        cooldownOverlay.fillAmount = remaining / total;
                    }
                    else
                    {
                        cooldownOverlay.fillAmount = remaining > 0 ? 1f : 0f; 
                    }
                }
                else
                {
                    cooldownOverlay.fillAmount = 0f;
                }
            }
        }
        
        public void SetTrackedAbility(AbilityHolder holder, SpecialAbility currentAbilityData)
        {
            trackedAbility = holder;
            if (iconImage != null && currentAbilityData != null)
            {
                iconImage.sprite = currentAbilityData.icon;
            }
        }
    }
}
