using UnityEngine;
using UnityEngine.UIElements;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Components.UI
{
    public class InteractionUI : MonoBehaviour
    {
        private VisualElement container;
        private Label promptLabel;
        private Label spellNameLabel;
        private Label spellDescLabel;

        public void Initialize(VisualElement root)
        {
            container = new VisualElement();
            container.style.position = Position.Absolute;
            container.style.bottom = Length.Percent(20);
            container.style.right = Length.Percent(5);
            container.style.alignItems = Align.Center;
            container.style.backgroundColor = new Color(0, 0, 0, 0.7f);
            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;
            container.style.paddingLeft = 20;
            container.style.paddingRight = 20;
            container.style.borderTopLeftRadius = 10;
            container.style.borderTopRightRadius = 10;
            container.style.borderBottomLeftRadius = 10;
            container.style.borderBottomRightRadius = 10;
            container.style.display = DisplayStyle.None;

            spellNameLabel = new Label();
            spellNameLabel.style.fontSize = 24;
            spellNameLabel.style.color = Color.white;
            spellNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            spellNameLabel.style.display = DisplayStyle.None;

            spellDescLabel = new Label();
            spellDescLabel.style.fontSize = 16;
            spellDescLabel.style.color = Color.yellow;
            spellDescLabel.style.whiteSpace = WhiteSpace.Normal;
            spellDescLabel.style.width = 300;
            spellDescLabel.style.unityTextAlign = TextAnchor.UpperCenter;
            spellDescLabel.style.display = DisplayStyle.None;
            spellDescLabel.style.marginBottom = 10;

            promptLabel = new Label();
            promptLabel.style.fontSize = 18;
            promptLabel.style.color = Color.white;

            container.Add(spellNameLabel);
            container.Add(spellDescLabel);
            container.Add(promptLabel);

            root.Add(container);
        }

        public void ShowPrompt(string actionText)
        {
            if (container == null) return;
            string keyName = KeyBindingManager.Instance.GetBinding(KeyBindingManager.GameAction.Interact).ToString();
            promptLabel.text = $"Pulsa [{keyName}] para {actionText}";
            
            spellNameLabel.style.display = DisplayStyle.None;
            spellDescLabel.style.display = DisplayStyle.None;
            container.style.display = DisplayStyle.Flex;
        }

        public void ShowSpellPrompt(string actionText, string spellName, string description)
        {
            if (container == null) return;
            string keyName = KeyBindingManager.Instance.GetBinding(KeyBindingManager.GameAction.Interact).ToString();
            promptLabel.text = $"Pulsa [{keyName}] para {actionText}";
            
            spellNameLabel.text = spellName;
            spellNameLabel.style.display = DisplayStyle.Flex;
            
            spellDescLabel.text = description;
            spellDescLabel.style.display = DisplayStyle.Flex;
            
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
