using UnityEngine;
using Blocks.Gameplay.Core;
using AnimalMagicRoyale.Spells;

namespace AnimalMagicRoyale.Core
{
    [System.Serializable]
    public struct LootBoxOpenedPayload
    {
        public GameObject player;
        public SpellData spell;
        public SpellTier tier;
    }

    [CreateAssetMenu(fileName = "LootBoxOpenedEvent", menuName = "Animal Magic Royale/Events/Loot Box Opened Event")]
    public class LootBoxOpenedEvent : GameEvent<LootBoxOpenedPayload>
    {
    }
}
