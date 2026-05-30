using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Core.Data;

namespace AnimalMagicRoyale.Components.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }
        
        public bool IsOpen => settingsPanel != null && settingsPanel.style.display == DisplayStyle.Flex;

        private UIDocument uiDocument;
        private VisualElement root;
        private VisualElement settingsPanel;

        // Tabs
        private Button tabGraphics;
        private Button tabAudio;
        private Button tabControls;
        private Button btnMainMenu;

        // Content Pages
        private VisualElement contentGraphics;
        private VisualElement contentAudio;
        private VisualElement contentControls;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance != null) return;
            GameObject prefab = Resources.Load<GameObject>("Settings");
            if (prefab != null)
            {
                Instantiate(prefab);
                Debug.Log("[SettingsManager] Auto-instantiated from Resources.");
            }
            else
            {
                Debug.LogWarning("[SettingsManager] Prefab not found in Resources! Please create one in Assets/_Project/Core/Prefabs/Resources/SettingsManager.");
                GameObject temp = new GameObject("SettingsManager_Temp");
                temp.AddComponent<UIDocument>();
                temp.AddComponent<SettingsManager>();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) return;
            
            // Forzar que el panel de ajustes se renderice siempre por encima del menú principal
            uiDocument.sortingOrder = 100;
            
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
                btnMainMenu = root.Q<Button>("BtnMainMenu");

                contentGraphics = root.Q<VisualElement>("ContentGraphics");
                contentAudio = root.Q<VisualElement>("ContentAudio");
                contentControls = root.Q<VisualElement>("ContentControls");

                if (tabGraphics != null) tabGraphics.clicked += () => SwitchTab("Graphics");
                if (tabAudio != null) tabAudio.clicked += () => SwitchTab("Audio");
                if (tabControls != null) tabControls.clicked += () => SwitchTab("Controls");
                
                if (btnMainMenu != null)
                {
                    btnMainMenu.clicked += ReturnToMainMenu;
                }

                SwitchTab("Graphics"); // Default
            }

            // Start hidden
            if (settingsPanel != null)
            {
                settingsPanel.style.display = DisplayStyle.None;
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ToggleSettings();
            }
        }

        public void ToggleSettings()
        {
            Debug.Log($"[SettingsManager] ToggleSettings called. Current state: IsOpen={IsOpen}");
            if (IsOpen)
            {
                HideSettings();
            }
            else
            {
                ShowSettings();
            }
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
            Debug.Log("[SettingsManager] ShowSettings invoked.");
            if (settingsPanel != null)
            {
                settingsPanel.style.display = DisplayStyle.Flex;
                LoadSettings();

                bool inGame = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Scene_MainMenu";

                if (btnMainMenu != null)
                {
                    btnMainMenu.style.display = inGame ? DisplayStyle.Flex : DisplayStyle.None;
                }

                // If in game (not main menu), free the cursor
                if (inGame)
                {
                    UnityEngine.Cursor.lockState = CursorLockMode.None;
                    UnityEngine.Cursor.visible = true;
                }
            }
        }

        public void HideSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.style.display = DisplayStyle.None;
                
                // If in game, restore cursor
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Scene_MainMenu")
                {
                    if (GameManager.Instance != null && GameManager.Instance.StateMachine.CurrentState is GameOverState)
                    {
                        UnityEngine.Cursor.lockState = CursorLockMode.None;
                        UnityEngine.Cursor.visible = true;
                    }
                    else
                    {
                        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                        UnityEngine.Cursor.visible = false;
                    }
                }
            }
        }

        private void LoadSettings()
        {
            if (PlayerPreferencesManager.Instance != null)
            {
                // Read from PlayerPreferencesManager.Instance.currentData
            }
        }

        private void SaveAndApply()
        {
            if (PlayerPreferencesManager.Instance != null)
            {
                PlayerPreferencesManager.Instance.SavePreferences();
            }

            ApplyGraphics();
            HideSettings();
        }

        public void ApplyGraphics()
        {
            Debug.Log("[SettingsManager] Graphics settings applied.");
        }

        private void ReturnToMainMenu()
        {
            HideSettings();
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadMainMenu();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Scene_MainMenu");
            }
        }
    }
}
