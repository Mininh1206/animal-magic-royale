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

            attackSource = gameObject.AddComponent<AudioSource>();
            attackSource.spatialBlend = 1f; // 3D sound
            attackSource.playOnAwake = false;
        }

        public void SetAudioData(CharacterAudioData newData)
        {
            audioData = newData;
        }

        public void PlayFootstep()
        {
            if (audioData == null || audioData.footstepClips == null || audioData.footstepClips.Length == 0) return;

            // Reproducir un sonido de paso aleatorio
            AudioClip clip = audioData.footstepClips[Random.Range(0, audioData.footstepClips.Length)];
            
            // Usar variacion de pitch para que no suene repetitivo
            footstepSource.pitch = Random.Range(0.9f, 1.1f);
            footstepSource.PlayOneShot(clip);
        }

        public void PlayAttackSound()
        {
            if (audioData == null || audioData.attackClips == null || audioData.attackClips.Length == 0) return;

            // Reproducir un sonido de ataque aleatorio
            AudioClip clip = audioData.attackClips[Random.Range(0, audioData.attackClips.Length)];
            
            // Variacion ligera de pitch
            attackSource.pitch = Random.Range(0.95f, 1.05f);
            attackSource.PlayOneShot(clip);
        }

        public float GetFootstepInterval()
        {
            return footstepInterval;
        }
    }
}
