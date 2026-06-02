using UnityEngine;
using AnimalMagicRoyale.Core.Data;

namespace AnimalMagicRoyale.Components.UI
{
    public class MenuMusicManager : MonoBehaviour
    {
        public static MenuMusicManager Instance { get; private set; }

        [SerializeField] private AudioClip[] menuTracks;
        private AudioSource audioSource;
        private int currentTrackIndex = -1;

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
                return;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.loop = false;
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.playOnAwake = false;
        }

        private void Start()
        {
            PlayRandomTrack();
        }

        private void Update()
        {
            if (audioSource == null) return;

            bool isMainMenu = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Scene_MainMenu";
            bool shouldPlay = isMainMenu || SettingsManager.MusicInMatchEnabled;

            if (!shouldPlay)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                    // Debug.Log("[MenuMusicManager] Stopping music because we are in match and it is disabled in settings.");
                }
                return;
            }

            audioSource.volume = SettingsManager.MusicVolume;

            if (!audioSource.isPlaying)
            {
                // Debug.Log("[MenuMusicManager] Track finished playing. Choosing a new random track.");
                PlayRandomTrack();
            }
        }

        private void PlayRandomTrack()
        {
            if (menuTracks == null || menuTracks.Length == 0) return;

            int newTrackIndex;
            if (menuTracks.Length == 1)
            {
                newTrackIndex = 0;
            }
            else
            {
                do
                {
                    newTrackIndex = Random.Range(0, menuTracks.Length);
                } while (newTrackIndex == currentTrackIndex);
            }

            currentTrackIndex = newTrackIndex;
            audioSource.clip = menuTracks[currentTrackIndex];
            audioSource.Play();
        }
    }
}
