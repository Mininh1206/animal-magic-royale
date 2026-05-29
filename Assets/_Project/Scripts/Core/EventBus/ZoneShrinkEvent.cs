using UnityEngine;
using Blocks.Gameplay.Core;

namespace AnimalMagicRoyale.Core
{
    [System.Serializable]
    public struct ZoneShrinkPayload
    {
        public int phaseIndex;
        public float currentRadius;
        public float targetRadius;
        public float duration;
    }

    [CreateAssetMenu(fileName = "ZoneShrinkEvent", menuName = "Animal Magic Royale/Events/Zone Shrink Event")]
    public class ZoneShrinkEvent : GameEvent<ZoneShrinkPayload>
    {
    }
}
