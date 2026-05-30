using UnityEngine;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Player
{
    public class PlayerIdleState : State
    {
        private PlayerController player;

        public PlayerIdleState(PlayerController player, StateMachine stateMachine) : base(stateMachine)
        {
            this.player = player;
        }

        public override void Enter()
        {
            Debug.Log("[FSM] Entering IdleState");
        }

        public override void Update()
        {
            if (player.AttackRequested)
            {
                Debug.Log("[PlayerIdleState] AttackRequested is TRUE. Changing to AttackState.");
                stateMachine.ChangeState(player.AttackState);
                return;
            }

            // Transición a MoveState si hay input
            if (player.MoveInput.sqrMagnitude > 0.01f)
            {
                stateMachine.ChangeState(player.MoveState);
            }
        }
    }
}
