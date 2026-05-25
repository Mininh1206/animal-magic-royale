using UnityEngine;
using Blocks.Gameplay.Core;

namespace AnimalMagicRoyale.Core
{
    public enum GameState
    {
        Waiting,
        Playing,
        GameOver
    }

    [CreateAssetMenu(fileName = "GameStateEvent", menuName = "Animal Magic Royale/Events/Game State Event")]
    public class GameStateEvent : GameEvent<GameState>
    {
    }
}
