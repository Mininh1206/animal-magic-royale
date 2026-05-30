using UnityEngine;
using UnityEngine.UIElements;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Components.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SettingsManager : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement root;
        private VisualElement settingsPanel;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            if (root != null)
            {
                settingsPanel = root.Q<VisualElement>("SettingsPanel");
                
                Button btnClose = root.Q<Button>("CloseSettingsBtn");
                Button btnApply = root.Q<Button>("ApplyBtn");
                Button btnCancel = root.Q<Button>("CancelBtn");

                if (btnClose != null) btnClose.clicked += HideSettings;
                if (btnApply != null) btnApply.clicked += SaveAndApply;
                if (btnCancel != null) btnCancel.clicked += HideSettings;

                // Bind other UI elements for graphics, sound, controls...
            }

            // Initially hidden
            HideSettings();
        }

        public void ShowSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.style.display = DisplayStyle.Flex;
                LoadSettings();
            }
        }

        public void HideSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.style.display = DisplayStyle.None;
            }
        }

        private void LoadSettings()
        {
            // Load from PlayerPrefs and update UI elements
        }

        private void SaveAndApply()
        {
            // Save to PlayerPrefs from UI elements
            PlayerPrefs.Save();

            // Apply graphics settings
            ApplyGraphics();

            HideSettings();
        }

        public void ApplyGraphics()
        {
            // Apply QualitySettings, Screen.SetResolution, etc. based on saved PlayerPrefs
            Debug.Log("[SettingsManager] Graphics settings applied.");
        }
    }
}
