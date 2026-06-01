using UnityEngine;
using AnimalMagicRoyale.Core.Data;

namespace AnimalMagicRoyale.Components
{
    public class CharacterSFXHandler : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private CharacterAudioData audioData;
        [SerializeField, Range(0.1f, 1f)] private float footstepInterval = 0.35f;

        private AudioSource footstepSource;
        private AudioSource attackSource;

        private void Awake()
        {
            // Crear AudioSources dinámicamente si no existen
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.spatialBlend = 1f; // 3D sound
            footstepSource.playOnAwake = false;
            footstepSource.rolloffMode = AudioRolloffMode.Linear;
            footstepSource.minDistance = 2f;
            footstepSource.maxDistance = 20f; // Distancia máxima de escucha para pasos

            attackSource = gameObject.AddComponent<AudioSource>();
            attackSource.spatialBlend = 1f; // 3D sound
            attackSource.playOnAwake = false;
            attackSource.rolloffMode = AudioRolloffMode.Linear;
            attackSource.minDistance = 3f;
            attackSource.maxDistance = 35f; // Distancia máxima de escucha para ataques
        }

        public void SetAudioData(CharacterAudioData newData)
        {
            audioData = newData;
        }

        public void SetFootstepsActive(bool active)
        {
            if (audioData == null || audioData.footstepClips == null || audioData.footstepClips.Length == 0) return;

            if (active)
            {
                if (!footstepSource.isPlaying)
                {
                    footstepSource.clip = audioData.footstepClips[0];
                    footstepSource.loop = true; // Looping the footstep file
                    footstepSource.pitch = Random.Range(0.95f, 1.05f);
                    footstepSource.volume = AnimalMagicRoyale.Components.UI.SettingsManager.SFXVolume;
                    footstepSource.Play();
                }
            }
            else
            {
                if (footstepSource.isPlaying)
                {
                    footstepSource.Stop();
                }
            }
        }

        public void PlayAttackSound()
        {
            if (audioData == null || audioData.attackClips == null || audioData.attackClips.Length == 0) return;

            // Reproducir un sonido de ataque aleatorio
            AudioClip clip = audioData.attackClips[Random.Range(0, audioData.attackClips.Length)];
            
            // Variacion ligera de pitch
            attackSource.pitch = Random.Range(0.95f, 1.05f);
            attackSource.volume = AnimalMagicRoyale.Components.UI.SettingsManager.SFXVolume;
            attackSource.PlayOneShot(clip);
        }

        public float GetFootstepInterval()
        {
            return footstepInterval;
        }
    }
}
