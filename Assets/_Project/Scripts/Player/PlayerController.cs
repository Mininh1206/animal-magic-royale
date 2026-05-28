using UnityEngine;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Player
{
    [RequireComponent(typeof(CharacterController))]
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

        [Header("Animation")]
        public Animator targetAnimator;

        // Componentes
        public CharacterController CharacterController { get; private set; }
        public StateMachine StateMachine { get; private set; }

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
        public float cameraDistance = 10f;
        public Vector3 cameraPivotOffset = new Vector3(0, 2f, 0);
        private float cameraPitch = 35f;

        public void Awake()
        {
            CharacterController = GetComponent<CharacterController>();
            StateMachine = new StateMachine();

            IdleState = new PlayerIdleState(this, StateMachine);
            MoveState = new PlayerMoveState(this, StateMachine);
            AttackState = new PlayerAttackState(this, StateMachine);
            StunnedState = new PlayerStunnedState(this, StateMachine);
        }

        public void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            StateMachine.Initialize(IdleState);
            
            if (UnityEngine.Camera.main != null)
            {
                cameraPitch = UnityEngine.Camera.main.transform.localEulerAngles.x;
            }
        }

        private void Update()
        {
            if (targetAnimator != null)
            {
                // Actualizamos el parámetro 'isRunning' del Animator.
                // Si te refieres a correr (sprint), puedes usar: IsSprinting && MoveInput.sqrMagnitude > 0.01f
                bool isRunning = MoveInput.sqrMagnitude > 0.01f;
                targetAnimator.SetBool("isRunning", isRunning);
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
            if (LookInput.sqrMagnitude >= 0.01f)
            {
                // Rotar jugador horizontalmente
                float yaw = LookInput.x * mouseSensitivity * Time.deltaTime;
                transform.Rotate(Vector3.up, yaw);

                // Rotar cámara verticalmente
                cameraPitch -= LookInput.y * mouseSensitivity * Time.deltaTime;
                cameraPitch = Mathf.Clamp(cameraPitch, -89f, 89f);
            }

            // Actualizar siempre la posición de la cámara para que orbite al jugador
            if (UnityEngine.Camera.main != null && UnityEngine.Camera.main.transform.parent == transform)
            {
                Quaternion localRotation = Quaternion.Euler(cameraPitch, 0, 0);
                Vector3 localPosition = cameraPivotOffset - (localRotation * Vector3.forward * cameraDistance);

                UnityEngine.Camera.main.transform.localPosition = localPosition;
                UnityEngine.Camera.main.transform.localRotation = localRotation;
            }
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
    }
}
