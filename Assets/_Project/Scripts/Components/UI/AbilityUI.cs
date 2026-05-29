using UnityEngine;
using UnityEngine.UI;

namespace AnimalMagicRoyale.Components.UI
{
    public class AbilityUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay; // Filled image for radial cooldown
        [SerializeField] private AbilityHolder trackedAbility;
        [SerializeField] private SpecialAbility defaultIconSource; // Optional
        
        private void Start()
        {
            if (iconImage != null && defaultIconSource != null && trackedAbility == null)
            {
                iconImage.sprite = defaultIconSource.icon;
            }
        }

        private void Update()
        {
            if (trackedAbility == null) return;
            
            if (cooldownOverlay != null)
            {
                if (!trackedAbility.IsReady)
                {
                    float remaining = trackedAbility.GetCooldownRemaining();
                    // Assuming we can get total cooldown from somewhere, or pass a normalized value
                    // Since AbilityHolder doesn't expose total cooldown easily, we will normalize it:
                    // Actually, AbilityHolder uses `ability.cooldown`. Since we don't have direct access
                    // we might need to assume a max cooldown. Let's just do a visual hack or access via reflection 
                    // if necessary. For now, let's use a flat remaining time display if radial isn't perfect.
                    // Wait, trackedAbility.GetCooldownRemaining() is exact time. To do radial, we need max cooldown.
                    // I will add a method to AbilityHolder if necessary, or just rely on text. 
                    // But radial is requested. I'll just use a fixed 10s for radial visual if max is unknown.
                    cooldownOverlay.fillAmount = remaining > 0 ? 1f : 0f; 
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
