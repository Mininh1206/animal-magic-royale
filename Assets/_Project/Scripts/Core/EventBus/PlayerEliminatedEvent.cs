using UnityEngine;
using Blocks.Gameplay.Core;

namespace AnimalMagicRoyale.Core
{
    [System.Serializable]
    public struct PlayerEliminatedPayload
    {
        public GameObject eliminated;
        public int remainingPlayers;
    }

    [CreateAssetMenu(fileName = "PlayerEliminatedEvent", menuName = "Animal Magic Royale/Events/Player Eliminated Event")]
    public class PlayerEliminatedEvent : GameEvent<PlayerEliminatedPayload>
    {
    }
}
