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

        // Componentes
        public CharacterController CharacterController { get; private set; }
        public StateMachine StateMachine { get; private set; }

        // Estados
        public PlayerIdleState IdleState { get; private set; }
        public PlayerMoveState MoveState { get; private set; }

        // Input
        public Vector2 MoveInput { get; set; }
        public bool IsSprinting { get; set; }
        public bool JumpRequested { get; set; }
        public float CameraYAngle { get; set; }

        // Estado interno
        public float VerticalVelocity { get; set; }
        public bool IsGrounded { get; private set; }
        public float RotationVelocity; // Usado por SmoothDampAngle

        private void Awake()
        {
            CharacterController = GetComponent<CharacterController>();
            StateMachine = new StateMachine();

            IdleState = new PlayerIdleState(this, StateMachine);
            MoveState = new PlayerMoveState(this, StateMachine);
        }

        private void Start()
        {
            StateMachine.Initialize(IdleState);
        }

        private void Update()
        {
            GroundedCheck();
            StateMachine.Update();
            ApplyGravity();
            ApplyVerticalMovement();
        }

        private void FixedUpdate()
        {
            StateMachine.FixedUpdate();
        }

        private void LateUpdate()
        {
            JumpRequested = false;
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z);
            IsGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
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
