using UnityEngine;

namespace AnimalMagicRoyale.Core.Data
{
    [CreateAssetMenu(fileName = "NewCharacterAudioData", menuName = "Animal Magic Royale/Data/Character Audio Data")]
    public class CharacterAudioData : ScriptableObject
    {
        [Header("Footsteps")]
        [Tooltip("Clips de sonido para los pasos del personaje.")]
        public AudioClip[] footstepClips;

        [Header("Attacks")]
        [Tooltip("Clips de sonido compartidos para los ataques.")]
        public AudioClip[] attackClips;
    }
}
