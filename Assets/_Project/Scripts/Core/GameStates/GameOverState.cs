using UnityEngine;

namespace AnimalMagicRoyale.Core
{
    public class GameOverState : State
    {
        private GameManager gameManager;

        public GameOverState(GameManager gameManager, StateMachine stateMachine) : base(stateMachine)
        {
            this.gameManager = gameManager;
        }

        public override void Enter()
        {
            Debug.Log("[GameOverState] Entered Game Over State.");
            
            // TODO: Stop ZoneManager
            // TODO: Disable player inputs
            
            if (gameManager.AlivePlayersCount == 1)
            {
                gameManager.Winner = gameManager.GetLastAlivePlayer();
                Debug.Log($"[GameOverState] Winner is {gameManager.Winner.name}");
            }
            else
            {
                Debug.Log("[GameOverState] It's a draw!");
            }
            
            if (gameManager.onGameStateChanged != null)
            {
                gameManager.onGameStateChanged.Raise(GameState.GameOver);
            }
        }
    }
}
