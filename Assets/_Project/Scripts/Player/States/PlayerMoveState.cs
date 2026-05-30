using UnityEngine;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Player
{
    public class PlayerMoveState : State
    {
        private PlayerController player;

        public PlayerMoveState(PlayerController player, StateMachine stateMachine) : base(stateMachine)
        {
            this.player = player;
        }

        public override void Enter()
        {
            Debug.Log("[FSM] Entering MoveState");
        }

        public override void Update()
        {
            if (player.AttackRequested)
            {
                Debug.Log("[PlayerMoveState] AttackRequested is TRUE. Changing to AttackState.");
                stateMachine.ChangeState(player.AttackState);
                return;
            }

            // Transición a IdleState si no hay input
            if (player.MoveInput.sqrMagnitude <= 0.01f)
            {
                stateMachine.ChangeState(player.IdleState);
                return;
            }

            Move();
        }

        private void Move()
        {
            bool effectiveSprinting = player.IsSprinting && player.MoveInput.y >= -0.1f;
            float targetSpeed = effectiveSprinting ? player.sprintSpeed : player.moveSpeed;

            // Mover hacia adelante/atrás y lados con W/S/A/D relativo a donde mira el personaje
            Vector3 targetDirection = player.transform.right * player.MoveInput.x + player.transform.forward * player.MoveInput.y;
            
            if (targetDirection.sqrMagnitude > 1f)
            {
                targetDirection.Normalize();
            }

            // Aplicar movimiento
            player.CharacterController.Move(targetDirection * (targetSpeed * Time.deltaTime));
        }
    }
}
