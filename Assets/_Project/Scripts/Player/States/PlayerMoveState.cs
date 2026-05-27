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
            float targetSpeed = player.IsSprinting ? player.sprintSpeed : player.moveSpeed;

            // Rotar el personaje con A/D (MoveInput.x)
            float turnSpeed = 150f; // Grados por segundo
            player.transform.Rotate(0, player.MoveInput.x * turnSpeed * Time.deltaTime, 0);

            // Mover hacia adelante/atrás con W/S (MoveInput.y) basado en hacia dónde mira el personaje
            Vector3 targetDirection = player.transform.forward * player.MoveInput.y;
            
            // Aplicar movimiento
            player.CharacterController.Move(targetDirection * (targetSpeed * Time.deltaTime));
        }
    }
}
