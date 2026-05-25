using UnityEngine;
using Blocks.Gameplay.Core;

namespace AnimalMagicRoyale.Core
{
    [CreateAssetMenu(fileName = "DeathEvent", menuName = "Animal Magic Royale/Events/Death Event")]
    public class DeathEvent : GameEvent<GameObject>
    {
    }
}
