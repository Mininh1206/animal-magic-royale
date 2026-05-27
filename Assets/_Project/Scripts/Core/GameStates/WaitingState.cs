using UnityEngine;

namespace AnimalMagicRoyale.Core
{
    public class WaitingState : State
    {
        private GameManager gameManager;

        public WaitingState(GameManager gameManager, StateMachine stateMachine) : base(stateMachine)
        {
            this.gameManager = gameManager;
        }

        public override void Enter()
        {
            Debug.Log("[WaitingState] Entered Waiting State. Waiting for match to start...");
            
            // TODO: Disable player inputs here
            
            if (gameManager.onGameStateChanged != null)
            {
                gameManager.onGameStateChanged.Raise(GameState.Waiting);
            }
        }

        public override void Update()
        {
            // The GameManager will handle the countdown and transition manually by calling StartMatch()
        }
    }
}
