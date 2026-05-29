using UnityEngine;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AnimalMagicRoyale.Components.HealthComponent))]
    [RequireComponent(typeof(AnimalMagicRoyale.Components.SpellInventory))]
    [RequireComponent(typeof(AnimalMagicRoyale.Components.AbilityHolder))]
    [RequireComponent(typeof(AnimalMagicRoyale.Components.CharacterAnimationHandler))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float sprintSpeed = 8f;
        public float rotationSmoothTime = 0.1f;
        public float gravity = -15f;
        public float jumpHeight = 1.2f;

        [Header("Ground Check")]
        public float groundedOffset = -0.14f;
        public float groundedRadius = 0.28f;
        public LayerMask groundLayers;

        // Components cache
        private AnimalMagicRoyale.Components.CharacterAnimationHandler _animHandler;

        // Componentes
        public CharacterController CharacterController { get; private set; }
        public StateMachine StateMachine { get; private set; }
        public HealthComponent HealthComponent { get; private set; }

        // Estados
        public PlayerIdleState IdleState { get; private set; }
        public PlayerMoveState MoveState { get; private set; }
        public PlayerAttackState AttackState { get; private set; }
        public PlayerStunnedState StunnedState { get; private set; }

        // Input
        public Vector2 MoveInput { get; set; }
        public Vector2 LookInput { get; set; }
        public bool IsSprinting { get; set; }
        public bool JumpRequested { get; set; }
        public float CameraYAngle { get; set; }
        public bool AttackRequested { get; set; }
        public int ActiveSlotChange { get; set; } = -1;
        public bool AbilityRequested { get; set; }
        public bool InteractRequested { get; set; }

        // Estado interno
        public float VerticalVelocity { get; set; }
        public bool IsGrounded { get; private set; }
        public float RotationVelocity; // Usado por SmoothDampAngle
        
        [Header("Mouse Look")]
        public float mouseSensitivity = 15f;

        public void Awake()
        {
            CharacterController = GetComponent<CharacterController>();
            StateMachine = new StateMachine();
            HealthComponent = GetComponent<HealthComponent>();

            IdleState = new PlayerIdleState(this, StateMachine);
            MoveState = new PlayerMoveState(this, StateMachine);
            AttackState = new PlayerAttackState(this, StateMachine);
            StunnedState = new PlayerStunnedState(this, StateMachine);

            _animHandler = GetComponent<AnimalMagicRoyale.Components.CharacterAnimationHandler>();
        }

        public void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            StateMachine.Initialize(IdleState);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterPlayer(gameObject);
            }
        }

        private void Update()
        {
            if (_animHandler != null)
            {
                bool effectiveSprinting = IsSprinting && MoveInput.y >= -0.1f;
                float speed = MoveInput.sqrMagnitude > 0.01f ? (effectiveSprinting ? sprintSpeed : moveSpeed) : 0f;
                _animHandler.UpdateLocomotion(speed, effectiveSprinting);
            }

            if (ActiveSlotChange != -1)
            {
                var inventory = GetComponent<AnimalMagicRoyale.Components.SpellInventory>();
                if (inventory != null)
                {
                    inventory.SelectSlot(ActiveSlotChange);
                }
            }

            if (AbilityRequested)
            {
                var abilityHolder = GetComponent<AnimalMagicRoyale.Components.AbilityHolder>();
                if (abilityHolder != null)
                {
                    abilityHolder.TryActivate();
                }
            }

            if (InteractRequested)
            {
                var colliders = Physics.OverlapSphere(transform.position, 3f);
                foreach (var col in colliders)
                {
                    var lootBox = col.GetComponent<AnimalMagicRoyale.Components.LootBox>();
                    if (lootBox != null)
                    {
                        if (lootBox.TryOpen(gameObject)) break;
                    }
                }
            }

            HandleMouseLook();
            GroundedCheck();
            StateMachine.Update();
            ApplyGravity();
            ApplyVerticalMovement();
        }

        private void HandleMouseLook()
        {
            if (LookInput.sqrMagnitude < 0.01f) return;

            // Solo rotar el jugador horizontalmente; Cinemachine gestiona el pitch de la cámara
            float yaw = LookInput.x * mouseSensitivity * Time.deltaTime;
            transform.Rotate(Vector3.up, yaw);
        }

        private void FixedUpdate()
        {
            StateMachine.FixedUpdate();
        }

        private void LateUpdate()
        {
            JumpRequested = false;
            AttackRequested = false;
            ActiveSlotChange = -1;
            AbilityRequested = false;
            InteractRequested = false;
        }

        private void GroundedCheck()
        {
            // Usar primariamente la comprobación del CharacterController que no depende de capas
            IsGrounded = CharacterController.isGrounded;
            
            // Fallback por si acaso con CheckSphere, solo si hay groundLayers configuradas
            if (!IsGrounded && groundLayers != 0)
            {
                Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z);
                IsGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
            }
        }

        private void ApplyGravity()
        {
            if (IsGrounded)
            {
                if (VerticalVelocity < 0.0f)
                {
                    VerticalVelocity = -2f;
                }

                if (JumpRequested)
                {
                    VerticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }
            else
            {
                if (VerticalVelocity < 53.0f) // Terminal velocity
                {
                    VerticalVelocity += gravity * Time.deltaTime;
                }
            }
        }

        private void ApplyVerticalMovement()
        {
            Vector3 verticalMove = new Vector3(0, VerticalVelocity, 0) * Time.deltaTime;
            CharacterController.Move(verticalMove);
        }

        public void TakeDamage(int amount)
        {
            HealthComponent.TakeDamage(amount);
        }

        public void Heal(int amount)
        {
            HealthComponent.Heal(amount);
        }
    }
}
