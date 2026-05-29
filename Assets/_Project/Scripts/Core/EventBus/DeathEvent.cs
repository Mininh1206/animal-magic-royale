using UnityEngine;
using Blocks.Gameplay.Core;

namespace AnimalMagicRoyale.Core
{
    [System.Serializable]
    public struct DeathPayload
    {
        public GameObject victim;
        public GameObject killer;
    }

    [CreateAssetMenu(fileName = "DeathEvent", menuName = "Animal Magic Royale/Events/Death Event")]
    public class DeathEvent : GameEvent<DeathPayload>
    {
    }
}
