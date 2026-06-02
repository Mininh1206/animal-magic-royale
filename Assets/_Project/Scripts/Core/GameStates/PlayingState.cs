using UnityEngine;

namespace AnimalMagicRoyale.Core
{
    public class PlayingState : State
    {
        private GameManager gameManager;

        public PlayingState(GameManager gameManager, StateMachine stateMachine) : base(stateMachine)
        {
            this.gameManager = gameManager;
        }

        public override void Enter()
        {
            // Debug.Log("[PlayingState] Entered Playing State. Match is now active!");
            
            if (ZoneManager.Instance != null)
            {
                ZoneManager.Instance.Activate();
                // Debug.Log("[PlayingState] ZoneManager activated.");
            }
            
            if (gameManager.onGameStateChanged != null)
            {
                gameManager.onGameStateChanged.Raise(GameState.Playing);
            }
            
            if (gameManager.onMatchStart != null)
            {
                gameManager.onMatchStart.Raise();
            }
        }

        public override void Update()
        {
            if (gameManager.AlivePlayersCount <= 1)
            {
                stateMachine.ChangeState(gameManager.GameOverState);
            }
        }
    }
}
