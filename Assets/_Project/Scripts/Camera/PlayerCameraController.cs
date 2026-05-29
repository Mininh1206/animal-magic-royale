using UnityEngine;
using UnityEngine.InputSystem;
using Blocks.Gameplay.Core;

namespace AnimalMagicRoyale.Camera
{
    /// <summary>
    /// Controlador de cámara third-person.
    /// Gestiona el look input y comunica el ángulo al PlayerController.
    /// </summary>
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private Player.PlayerController playerController;
        [SerializeField] private float lookSensitivity = 0.5f;
        [SerializeField] private float verticalLookLimit = 70f;

        private GameplayInputSystem_Actions inputActions;
        private Vector2 lookInput;
        
        public float HorizontalAngle { get; private set; }
        public float VerticalAngle { get; private set; }

        private void Awake()
        {
            inputActions = new GameplayInputSystem_Actions();
        }

        private void OnEnable()
        {
            inputActions.Player.Enable();
            inputActions.Player.Look.performed += HandleLook;
            inputActions.Player.Look.canceled += HandleLook;

            // Bloquear el cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            inputActions.Player.Disable();
            inputActions.Player.Look.performed -= HandleLook;
            inputActions.Player.Look.canceled -= HandleLook;

            // Liberar el cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HandleLook(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }

        private void LateUpdate()
        {
            // Actualizar ángulos
            HorizontalAngle += lookInput.x * lookSensitivity;
            VerticalAngle = Mathf.Clamp(VerticalAngle - (lookInput.y * lookSensitivity), -verticalLookLimit, verticalLookLimit);

            // Rotar el transform (CameraTarget) que Cinemachine va a seguir
            transform.rotation = Quaternion.Euler(VerticalAngle, HorizontalAngle, 0f);

            // Informar al jugador de la rotación de cámara actual
            if (playerController != null)
            {
                playerController.CameraYAngle = HorizontalAngle;
            }
        }
    }
}
