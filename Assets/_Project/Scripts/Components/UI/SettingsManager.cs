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

        // UI Controls
        private DropdownField dropdownResolution;
        private Toggle toggleFullscreen;
        private Slider sliderGlobal;
        private Slider sliderMusic;
        private Slider sliderSpells;
        private Slider sliderEnvironment;

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
                if (btnReset != null) btnReset.clicked += ResetToDefaults;

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

                dropdownResolution = root.Q<DropdownField>("dropdown-resolution");
                toggleFullscreen = root.Q<Toggle>("toggle-fullscreen");
                sliderGlobal = root.Q<Slider>("slider-global");
                sliderMusic = root.Q<Slider>("slider-music");
                sliderSpells = root.Q<Slider>("slider-spells");
                sliderEnvironment = root.Q<Slider>("slider-environment");

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
                    btnMainMenu.style.display = DisplayStyle.Flex;
                    var btnLabel = btnMainMenu.Q<Label>();
                    if (btnLabel != null) btnLabel.text = inGame ? "Volver al Menú Principal" : "Salir del Juego";
                    else btnMainMenu.text = inGame ? "Volver al Menú Principal" : "Salir del Juego";
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
            var data = PlayerPreferencesManager.Instance?.currentData;
            
            if (toggleFullscreen != null) toggleFullscreen.value = data != null ? data.isFullscreen : Screen.fullScreen;
            if (sliderGlobal != null) sliderGlobal.value = data != null ? data.masterVolume / 100f : AudioListener.volume;
            
            // Temporary default logic for other sliders
            if (sliderMusic != null) sliderMusic.value = 1f;
            if (sliderSpells != null) sliderSpells.value = 1f;
            if (sliderEnvironment != null) sliderEnvironment.value = 1f;
            
            PopulateResolutions();
            if (dropdownResolution != null && data != null && dropdownResolution.choices.Count > 0)
            {
                if (data.resolutionIndex >= 0 && data.resolutionIndex < dropdownResolution.choices.Count)
                {
                    dropdownResolution.index = data.resolutionIndex;
                }
            }
        }

        private void ResetToDefaults()
        {
            if (toggleFullscreen != null) toggleFullscreen.value = true;
            if (sliderGlobal != null) sliderGlobal.value = 1f;
            if (sliderMusic != null) sliderMusic.value = 1f;
            if (sliderSpells != null) sliderSpells.value = 1f;
            if (sliderEnvironment != null) sliderEnvironment.value = 1f;
            
            PopulateResolutions();
            if (dropdownResolution != null && dropdownResolution.choices.Count > 0)
            {
                // Set native resolution (usually the last in the populated list)
                dropdownResolution.index = dropdownResolution.choices.Count - 1;
            }
            SaveAndApply();
        }

        private void PopulateResolutions()
        {
            if (dropdownResolution == null) return;
            var resolutions = Screen.resolutions;
            var choices = new System.Collections.Generic.List<string>();
            foreach (var res in resolutions)
            {
                string label = $"{res.width}x{res.height}";
                if (!choices.Contains(label)) choices.Add(label);
            }
            if (choices.Count > 0)
            {
                dropdownResolution.choices = choices;
            }
            else
            {
                dropdownResolution.choices = new System.Collections.Generic.List<string> { $"{Screen.width}x{Screen.height}" };
            }
        }

        private void SaveAndApply()
        {
            if (PlayerPreferencesManager.Instance != null)
            {
                var data = PlayerPreferencesManager.Instance.currentData;
                if (toggleFullscreen != null) data.isFullscreen = toggleFullscreen.value;
                if (sliderGlobal != null) data.masterVolume = sliderGlobal.value * 100f;
                if (dropdownResolution != null && dropdownResolution.index >= 0) data.resolutionIndex = dropdownResolution.index;
                PlayerPreferencesManager.Instance.SavePreferences();
            }

            ApplyGraphics();
            ApplyAudio();
            HideSettings();
        }

        public void ApplyGraphics()
        {
            if (toggleFullscreen != null)
            {
                Screen.fullScreen = toggleFullscreen.value;
            }
            
            if (dropdownResolution != null)
            {
                string resStr = dropdownResolution.value;
                if (!string.IsNullOrEmpty(resStr) && resStr.Contains("x"))
                {
                    string[] parts = resStr.Split('x');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                    {
                        Screen.SetResolution(w, h, Screen.fullScreen);
                    }
                }
            }
            Debug.Log("[SettingsManager] Graphics settings applied.");
        }

        public void ApplyAudio()
        {
            if (sliderGlobal != null)
            {
                AudioListener.volume = sliderGlobal.value;
            }
            // Logic for Music, Spells, Environment Mixers will go here
            Debug.Log("[SettingsManager] Audio settings applied.");
        }

        private void ReturnToMainMenu()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Scene_MainMenu")
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }

            HideSettings();
            if (SceneLoaderManager.Instance != null)
            {
                SceneLoaderManager.Instance.LoadMainMenu();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Scene_MainMenu");
            }
        }
    }
}
