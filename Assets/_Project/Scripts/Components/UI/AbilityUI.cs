using UnityEngine;
using AnimalMagicRoyale.Core;
using UnityEngine.UIElements;

namespace AnimalMagicRoyale.Components.UI
{
    public class AbilityUI : MonoBehaviour
    {
        private VisualElement iconImage;
        private VisualElement cooldownOverlay;
        private Label keyText;

        [SerializeField] private AbilityHolder trackedAbility;
        [SerializeField] private SpecialAbility defaultIconSource; // Optional
        [SerializeField] private KeyBindingManager.GameAction slotAction = KeyBindingManager.GameAction.Ability;
        
        public void Initialize(VisualElement root)
        {
            var container = root.Q<VisualElement>("ability-icon");
            if (container != null)
            {
                // Let's assume the UXML will have these children or we create them
                iconImage = container.Q<VisualElement>("icon");
                if (iconImage == null) { iconImage = new VisualElement { name = "icon" }; iconImage.AddToClassList("slot-icon"); container.Add(iconImage); }
                
                // Asegurarnos de que el icono rellene completamente el recuadro sin verse empujado por otros elementos
                if (iconImage != null)
                {
                    iconImage.style.unityBackgroundScaleMode = ScaleMode.StretchToFill;
                    iconImage.style.width = Length.Percent(100);
                    iconImage.style.height = Length.Percent(100);
                    iconImage.style.position = Position.Absolute;
                    iconImage.style.top = 0;
                    iconImage.style.left = 0;
                    iconImage.style.right = 0;
                    iconImage.style.bottom = 0;
                }
                
                cooldownOverlay = container.Q<VisualElement>("cooldown");
                if (cooldownOverlay == null) { cooldownOverlay = new VisualElement { name = "cooldown" }; cooldownOverlay.AddToClassList("slot-cooldown"); container.Add(cooldownOverlay); }
                
                keyText = container.Q<Label>("key");
                if (keyText == null) { keyText = new Label { name = "key", text = "Q" }; keyText.AddToClassList("slot-key"); container.Add(keyText); }
            }
            
            UpdateIcon();
        }

        private void UpdateIcon()
        {
            if (iconImage != null && defaultIconSource != null && trackedAbility == null)
            {
                iconImage.style.backgroundImage = new UnityEngine.UIElements.StyleBackground(defaultIconSource.icon);
            }
            else if (iconImage != null && trackedAbility != null && trackedAbility.Ability != null)
            {
                iconImage.style.backgroundImage = new UnityEngine.UIElements.StyleBackground(trackedAbility.Ability.icon);
            }
        }

        private void Start()
        {
            UpdateIcon();
        }

        private void Update()
        {
            if (keyText != null && KeyBindingManager.Instance != null)
            {
                string keyName = KeyBindingManager.Instance.GetBinding(slotAction).ToString();
                keyText.text = keyName.Replace("Digit", "");
                keyText.style.color = (trackedAbility != null && trackedAbility.IsReady) ? Color.green : Color.white;
            }

            if (trackedAbility == null) return;
            
            if (cooldownOverlay != null)
            {
                if (!trackedAbility.IsReady)
                {
                    cooldownOverlay.style.display = DisplayStyle.Flex;
                    float remaining = trackedAbility.GetCooldownRemaining();
                    float total = trackedAbility.TotalCooldown;
                    if (total > 0f)
                    {
                        cooldownOverlay.style.height = Length.Percent((remaining / total) * 100f);
                    }
                    else
                    {
                        cooldownOverlay.style.height = Length.Percent(remaining > 0 ? 100f : 0f); 
                    }
                }
                else
                {
                    cooldownOverlay.style.display = DisplayStyle.None;
                }
            }
        }
        
        public void SetTrackedAbility(AbilityHolder holder, SpecialAbility currentAbilityData)
        {
            trackedAbility = holder;
            if (iconImage != null && currentAbilityData != null)
            {
                iconImage.style.backgroundImage = new StyleBackground(currentAbilityData.icon);
            }
        }
    }
}
