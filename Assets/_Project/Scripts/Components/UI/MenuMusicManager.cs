using UnityEngine;
using AnimalMagicRoyale.Core.Data;

namespace AnimalMagicRoyale.Components.UI
{
    public class MenuMusicManager : MonoBehaviour
    {
        [SerializeField] private AudioClip[] menuTracks;
        private AudioSource audioSource;
        private int currentTrackIndex = -1;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.loop = true;
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.playOnAwake = false;
        }

        private void Start()
        {
            PlayRandomTrack();
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
