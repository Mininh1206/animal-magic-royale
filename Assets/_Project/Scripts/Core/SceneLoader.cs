using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using AnimalMagicRoyale.Core.Data;

namespace AnimalMagicRoyale.Core
{
    [RequireComponent(typeof(UIDocument))]
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        private UIDocument uiDocument;
        private VisualElement loadingContainer;
        private VisualElement progressBarFill;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance != null) return;
            
            // Try to load prefab from Assets/_Project/Core/Prefabs/Resources/SceneLoader.prefab
            GameObject prefab = Resources.Load<GameObject>("SceneLoader");
            if (prefab != null)
            {
                Instantiate(prefab);
                Debug.Log("[SceneLoader] Auto-instantiated from Resources.");
            }
            else
            {
                Debug.LogWarning("[SceneLoader] Prefab not found in Resources! Please create one in Assets/_Project/Core/Prefabs/Resources/SceneLoader.");
                // Create a temporary one just so it doesn't break
                GameObject temp = new GameObject("SceneLoader_Temp");
                temp.AddComponent<UIDocument>();
                temp.AddComponent<SceneLoader>();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                uiDocument = GetComponent<UIDocument>();
                if (uiDocument != null && uiDocument.rootVisualElement != null)
                {
                    loadingContainer = uiDocument.rootVisualElement.Q<VisualElement>("LoadingContainer");
                    progressBarFill = uiDocument.rootVisualElement.Q<VisualElement>("ProgressBarFill");
                    
                    if (loadingContainer != null) loadingContainer.style.display = DisplayStyle.None;
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadScene(string sceneName)
        {
            Debug.Log($"[SceneLoader] Loading scene: {sceneName}");
            StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
        }
        
        private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
        {
            if (loadingContainer != null) 
            {
                loadingContainer.style.display = DisplayStyle.Flex;
                if (progressBarFill != null) progressBarFill.style.width = Length.Percent(0);
            }

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            if (asyncLoad == null)
            {
                Debug.LogError($"[SceneLoader] Failed to load scene {sceneName}. Is it in Build Settings?");
                yield break;
            }
            
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                
                if (progressBarFill != null)
                {
                    progressBarFill.style.width = Length.Percent(progress * 100f);
                }

                if (asyncLoad.progress >= 0.9f)
                {
                    // Small artificial delay to see the full bar if it loads too fast
                    yield return new WaitForSeconds(0.5f); 
                    asyncLoad.allowSceneActivation = true;
                }
                
                yield return null;
            }
            
            if (loadingContainer != null) 
            {
                loadingContainer.style.display = DisplayStyle.None;
            }
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
