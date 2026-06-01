using UnityEngine;
using UnityEngine.UIElements;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Spells;

namespace AnimalMagicRoyale.Components.UI
{
    public class InteractionUI : MonoBehaviour
    {
        private VisualElement container;
        private Label titleLabel;
        private Label descLabel;
        private Label promptLabel;

        public void Initialize(VisualElement root)
        {
            container = root.Q<VisualElement>("interaction-container");
            if (container != null)
            {
                titleLabel = container.Q<Label>("interaction-title");
                descLabel = container.Q<Label>("interaction-desc");
                promptLabel = container.Q<Label>("interaction-prompt");
                
                container.style.display = DisplayStyle.None;
            }
        }

        public void ShowPrompt(string actionText)
        {
            if (container == null) return;
            string keyName = KeyBindingManager.Instance.GetBinding(KeyBindingManager.GameAction.Interact).ToString();
            promptLabel.text = $"Pulsa [{keyName}] para {actionText}";
            
            titleLabel.text = actionText;
            titleLabel.style.color = Color.white;
            titleLabel.style.display = DisplayStyle.Flex;
            
            descLabel.style.display = DisplayStyle.None;
            
            container.style.display = DisplayStyle.Flex;
        }

        public void ShowSpellPrompt(string actionText, SpellData spellData)
        {
            if (container == null || spellData == null) return;
            string keyName = KeyBindingManager.Instance.GetBinding(KeyBindingManager.GameAction.Interact).ToString();
            promptLabel.text = $"Pulsa [{keyName}] para {actionText}";
            
            titleLabel.text = spellData.spellName;
            titleLabel.style.color = spellData.spellColor;
            titleLabel.style.display = DisplayStyle.Flex;
            
            descLabel.text = $"Tier: {spellData.tier}\n{spellData.description}";
            
            // Build effects string
            string effectsStr = "";
            if (spellData.effects != null && spellData.effects.Count > 0)
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
            if (!string.IsNullOrEmpty(effectsStr))
            {
                descLabel.text += $"\n\nEfectos:\n{effectsStr}";
            }

            descLabel.style.display = DisplayStyle.Flex;
            container.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            if (container != null)
            {
                container.style.display = DisplayStyle.None;
            }
        }
    }
}
