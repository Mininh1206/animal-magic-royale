using UnityEngine;
using UnityEngine.SceneManagement;
using AnimalMagicRoyale.Core.Data;

namespace AnimalMagicRoyale.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

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

        public void LoadScene(string sceneName)
        {
            Debug.Log($"[SceneLoader] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }

        public void LoadMainMenu()
        {
            // Reset player setup data when going back to main menu
            PlayerSetupData.Reset();
            LoadScene("Scene_MainMenu");
        }

        public void LoadMap(MapData mapData)
        {
            if (mapData != null && !string.IsNullOrEmpty(mapData.sceneName))
            {
                LoadScene(mapData.sceneName);
            }
            else
            {
                Debug.LogError("[SceneLoader] MapData is null or sceneName is empty!");
            }
        }
    }
}
