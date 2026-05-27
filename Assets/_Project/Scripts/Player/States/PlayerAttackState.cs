using UnityEngine;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Player
{
    public class PlayerAttackState : State
    {
        private PlayerController player;
        private SpellInventory inventory;
        private float attackDuration = 0.5f; 
        private float startTime;

        public PlayerAttackState(PlayerController player, StateMachine stateMachine) : base(stateMachine)
        {
            this.player = player;
            this.inventory = player.GetComponent<SpellInventory>();
        }

        public override void Enter()
        {
            startTime = Time.time;

            if (inventory != null)
            {
                Vector3 direction = player.transform.forward;
                inventory.TryCast(player.gameObject, direction);
            }
        }

        public override void Update()
        {
            // Lógica de movimiento (Tank Controls) mientras se ataca
            float targetSpeed = player.IsSprinting ? player.sprintSpeed : player.moveSpeed;
            float turnSpeed = 150f;
            player.transform.Rotate(0, player.MoveInput.x * turnSpeed * Time.deltaTime, 0);
            Vector3 targetDirection = player.transform.forward * player.MoveInput.y;
            player.CharacterController.Move(targetDirection * (targetSpeed * Time.deltaTime));

            // Lógica de finalización de ataque
            if (Time.time - startTime >= attackDuration)
            {
                if (player.MoveInput.sqrMagnitude > 0.01f)
                {
                    stateMachine.ChangeState(player.MoveState);
                }
                else
                {
                    stateMachine.ChangeState(player.IdleState);
                }
            }
        }
    }
}
