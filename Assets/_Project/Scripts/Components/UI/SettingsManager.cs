using UnityEngine;
using UnityEngine.UIElements;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Core.Data;

namespace AnimalMagicRoyale.Components.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SettingsManager : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement root;
        private VisualElement settingsPanel;

        // Tabs
        private Button tabGraphics;
        private Button tabAudio;
        private Button tabControls;

        // Content Pages
        private VisualElement contentGraphics;
        private VisualElement contentAudio;
        private VisualElement contentControls;

        private void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;

            if (root != null)
            {
                settingsPanel = root.Q<VisualElement>("SettingsPanel");
                
                Button btnClose = root.Q<Button>("CloseSettingsBtn");
                Button btnApply = root.Q<Button>("ApplyBtn");
                Button btnReset = root.Q<Button>("ResetBtn");

                if (btnClose != null) btnClose.clicked += HideSettings;
                if (btnApply != null) btnApply.clicked += SaveAndApply;
                if (btnReset != null) btnReset.clicked += LoadSettings;

                // Bind tabs
                tabGraphics = root.Q<Button>("TabGraphics");
                tabAudio = root.Q<Button>("TabAudio");
                tabControls = root.Q<Button>("TabControls");

                contentGraphics = root.Q<VisualElement>("ContentGraphics");
                contentAudio = root.Q<VisualElement>("ContentAudio");
                contentControls = root.Q<VisualElement>("ContentControls");

                if (tabGraphics != null) tabGraphics.clicked += () => SwitchTab("Graphics");
                if (tabAudio != null) tabAudio.clicked += () => SwitchTab("Audio");
                if (tabControls != null) tabControls.clicked += () => SwitchTab("Controls");

                SwitchTab("Graphics"); // Default
            }

            HideSettings();
        }

        private void SwitchTab(string tabName)
        {
            if (tabGraphics != null) tabGraphics.RemoveFromClassList("settings-tab-active");
            if (tabAudio != null) tabAudio.RemoveFromClassList("settings-tab-active");
            if (tabControls != null) tabControls.RemoveFromClassList("settings-tab-active");

            if (contentGraphics != null) contentGraphics.style.display = DisplayStyle.None;
            if (contentAudio != null) contentAudio.style.display = DisplayStyle.None;
            if (contentControls != null) contentControls.style.display = DisplayStyle.None;

            if (tabName == "Graphics")
            {
                if (tabGraphics != null) tabGraphics.AddToClassList("settings-tab-active");
                if (contentGraphics != null) contentGraphics.style.display = DisplayStyle.Flex;
            }
            else if (tabName == "Audio")
            {
                if (tabAudio != null) tabAudio.AddToClassList("settings-tab-active");
                if (contentAudio != null) contentAudio.style.display = DisplayStyle.Flex;
            }
            else if (tabName == "Controls")
            {
                if (tabControls != null) tabControls.AddToClassList("settings-tab-active");
                if (contentControls != null) contentControls.style.display = DisplayStyle.Flex;
            }
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
            if (PlayerPreferencesManager.Instance != null)
            {
                // Read from PlayerPreferencesManager.Instance.currentData
                // and update UI elements here
            }
        }

        private void SaveAndApply()
        {
            if (PlayerPreferencesManager.Instance != null)
            {
                // Update PlayerPreferencesManager.Instance.currentData from UI elements here
                PlayerPreferencesManager.Instance.SavePreferences();
            }

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
