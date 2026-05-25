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
            Vector3 inputDirection = new Vector3(player.MoveInput.x, 0.0f, player.MoveInput.y).normalized;

            // Mover relativo a la rotación de la cámara (CameraYAngle)
            float targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + player.CameraYAngle;
            
            // Rotar el personaje
            float rotation = Mathf.SmoothDampAngle(player.transform.eulerAngles.y, targetRotation, ref player.RotationVelocity, player.rotationSmoothTime);
            player.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            // Mover
            Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
            player.CharacterController.Move(targetDirection.normalized * (targetSpeed * Time.deltaTime));
        }
    }
}
