using UnityEngine;

namespace AnimalMagicRoyale.Core
{
    [CreateAssetMenu(fileName = "NewZonePhase", menuName = "Animal Magic Royale/Data/Zone Phase Data")]
    public class ZonePhaseData : ScriptableObject
    {
        [Header("Size & Timing")]
        public float startDiameter = 200f;
        public float endDiameter = 100f;
        public float shrinkDuration = 30f;
        public float waitBeforeShrink = 60f;

        [Header("Damage")]
        public float baseDamage = 1f;
        public float damageMultiplier = 0.1f; // +10% per second outside
    }
}
