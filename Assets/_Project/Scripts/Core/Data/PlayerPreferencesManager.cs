using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AnimalMagicRoyale.Core.Data
{
    [System.Serializable]
    public class PlayerPreferencesData
    {
        public int animalTypeId = 0;
        public string skinName = "";
        
        // Graphics
        public int resolutionIndex = 0;
        public float brightness = 75f;
        public bool isFullscreen = true;
        
        // Audio
        public float masterVolume = 100f;
        public float musicVolume = 100f;
        public float sfxVolume = 100f;
        public float envVolume = 100f;
        public bool musicInMatch = false;
    }

    public class PlayerPreferencesManager : MonoBehaviour
    {
        public static PlayerPreferencesManager Instance { get; private set; }
        
        public PlayerPreferencesData currentData = new PlayerPreferencesData();
        
        private string SavePath => Path.Combine(Application.persistentDataPath, "playerPrefs.json");
        private string HashPath => Path.Combine(Application.persistentDataPath, "playerPrefs.hash");
        
        // Secret key to mix in the hash so players can't just generate a new MD5
        private const string SALT = "AMR_PREFS_SALT_2026";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance != null) return;
            GameObject temp = new GameObject("PlayerPreferencesManager");
            Instance = temp.AddComponent<PlayerPreferencesManager>();
            DontDestroyOnLoad(temp);
            Instance.LoadPreferences();
            Instance.ApplyPreferences();
            // Debug.Log("[PlayerPreferences] Auto-instantiated BeforeSceneLoad.");
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadPreferences();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void LoadPreferences()
        {
            if (File.Exists(SavePath) && File.Exists(HashPath))
            {
                string json = File.ReadAllText(SavePath);
                string savedHash = File.ReadAllText(HashPath);
                string currentHash = GenerateHash(json);
                
                if (savedHash == currentHash)
                {
                    currentData = JsonUtility.FromJson<PlayerPreferencesData>(json);
                    // Debug.Log("[PlayerPreferences] Preferences loaded successfully.");
                }
                else
                {
                    // Debug.LogWarning("[PlayerPreferences] Hash mismatch! File may have been tampered with. Creating new preferences.");
                    currentData = new PlayerPreferencesData();
                    if (Screen.resolutions != null && Screen.resolutions.Length > 0)
                    {
                        currentData.resolutionIndex = Screen.resolutions.Length - 1;
                    }
                    SavePreferences();
                }
            }
            else
            {
                // No file exists
                currentData = new PlayerPreferencesData();
                if (Screen.resolutions != null && Screen.resolutions.Length > 0)
                {
                    currentData.resolutionIndex = Screen.resolutions.Length - 1;
                }
                SavePreferences();
            }
        }

        public void ApplyPreferences()
        {
            Screen.fullScreen = currentData.isFullscreen;
            AudioListener.volume = currentData.masterVolume / 100f;
            
            var resolutions = Screen.resolutions;
            if (resolutions != null && resolutions.Length > 0)
            {
                if (currentData.resolutionIndex >= 0 && currentData.resolutionIndex < resolutions.Length)
                {
                    var res = resolutions[currentData.resolutionIndex];
                    Screen.SetResolution(res.width, res.height, currentData.isFullscreen);
                }
            }
        }
        
        public void SavePreferences()
        {
            string json = JsonUtility.ToJson(currentData, true);
            string hash = GenerateHash(json);
            
            File.WriteAllText(SavePath, json);
            File.WriteAllText(HashPath, hash);
            // Debug.Log("[PlayerPreferences] Preferences saved.");
        }
        
        private string GenerateHash(string payload)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(payload + SALT);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
