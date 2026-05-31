using UnityEngine;
using UnityEngine.UIElements;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Components.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class GameOverManager : MonoBehaviour
    {
        public static GameOverManager Instance { get; private set; }

        private UIDocument uiDocument;
        private VisualElement root;
        private VisualElement overlay;
        private Label title;
        private Label message;
        private Button btnMainMenu;

        private GameStateEvent onGameStateChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance != null) return;
            GameObject prefab = Resources.Load<GameObject>("GameOver");
            if (prefab != null)
            {
                Instantiate(prefab);
                Debug.Log("[GameOverManager] Auto-instantiated from Resources.");
            }
            else
            {
                Debug.LogWarning("[GameOverManager] Prefab not found in Resources! Please create one in Assets/_Project/Core/Prefabs/Resources/GameOverManager.");
                GameObject temp = new GameObject("GameOverManager_Temp");
                temp.AddComponent<UIDocument>();
                temp.AddComponent<GameOverManager>();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                onGameStateChanged = Resources.Load<GameStateEvent>("Events/GameStateEvent");
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
            
            root = uiDocument.rootVisualElement;

            if (root != null)
            {
                overlay = root.Q<VisualElement>("GameOverOverlay");
                title = root.Q<Label>("GameOverTitle");
                message = root.Q<Label>("GameOverMessage");
                btnMainMenu = root.Q<Button>("BtnMainMenu");

                if (btnMainMenu != null)
                {
                    btnMainMenu.clicked += ReturnToMainMenu;
                }
            }

            if (overlay != null)
            {
                overlay.style.display = DisplayStyle.None;
            }

            if (onGameStateChanged != null)
            {
                onGameStateChanged.RegisterListener(HandleGameStateChanged);
            }
        }

        private void OnDisable()
        {
            if (onGameStateChanged != null)
            {
                onGameStateChanged.UnregisterListener(HandleGameStateChanged);
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                ShowGameOver();
            }
            else
            {
                HideGameOver();
            }
        }

        public void ShowGameOver()
        {
            if (overlay != null)
            {
                overlay.style.display = DisplayStyle.Flex;

                if (title != null && GameManager.Instance != null)
                {
                    if (GameManager.Instance.Winner != null)
                    {
                        title.text = "¡FIN DE LA PARTIDA!";
                    }
                    else
                    {
                        title.text = "GAME OVER";
                    }
                }

                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
            }
        }

        public void HideGameOver()
        {
            if (overlay != null)
            {
                overlay.style.display = DisplayStyle.None;
            }
        }

        private void ReturnToMainMenu()
        {
            HideGameOver();
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
