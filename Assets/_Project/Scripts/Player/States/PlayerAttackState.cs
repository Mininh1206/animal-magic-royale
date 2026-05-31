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
            Debug.Log("[PlayerAttackState] Enter() called!");
            startTime = Time.time;

            if (inventory != null)
            {
                Vector3 firePos = inventory.FirePoint != null ? inventory.FirePoint.position : player.transform.position + Vector3.up * 1f;
                Vector3 direction = AimHelper.GetAimDirection(firePos, player.gameObject);
                inventory.TryCast(player.gameObject, direction);
            }
            else
            {
                Debug.LogError("[PlayerAttackState] ERROR: inventory is NULL! Cannot call TryCast.");
            }
        }

        public override void Update()
        {
            bool effectiveSprinting = player.IsSprinting && player.MoveInput.y >= -0.1f;
            float targetSpeed = effectiveSprinting ? player.sprintSpeed : player.moveSpeed;
            
            Vector3 targetDirection = player.transform.right * player.MoveInput.x + player.transform.forward * player.MoveInput.y;
            if (targetDirection.sqrMagnitude > 1f)
            {
                targetDirection.Normalize();
            }
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
