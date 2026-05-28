using UnityEngine;
using UnityEngine.InputSystem;
using Blocks.Gameplay.Core;

namespace AnimalMagicRoyale.Player
{
    /// <summary>
    /// Captura inputs del jugador mediante el Input System y los inyecta
    /// directamente en el PlayerController. Sin dependencia de red.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;

        private GameplayInputSystem_Actions inputActions;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }
            inputActions = new GameplayInputSystem_Actions();
        }

        private void OnEnable()
        {
            inputActions.Player.Enable();

            inputActions.Player.Move.performed += HandleMove;
            inputActions.Player.Move.canceled += HandleMove;

            inputActions.Player.Jump.performed += HandleJump;

            inputActions.Player.Sprint.started += HandleSprint;
            inputActions.Player.Sprint.canceled += HandleSprint;
        }

        private void OnDisable()
        {
            inputActions.Player.Disable();

            inputActions.Player.Move.performed -= HandleMove;
            inputActions.Player.Move.canceled -= HandleMove;

            inputActions.Player.Jump.performed -= HandleJump;

            inputActions.Player.Sprint.started -= HandleSprint;
            inputActions.Player.Sprint.canceled -= HandleSprint;
        }

        private void HandleMove(InputAction.CallbackContext context)
        {
            if (playerController != null)
            {
                playerController.MoveInput = context.ReadValue<Vector2>();
            }
        }

        private void HandleJump(InputAction.CallbackContext context)
        {
            if (playerController != null)
            {
                playerController.JumpRequested = true;
            }
        }

        private void HandleSprint(InputAction.CallbackContext context)
        {
            if (playerController != null)
            {
                playerController.IsSprinting = context.ReadValueAsButton();
            }
        }

        private void Update()
        {
            if (playerController != null)
            {
                if (Mouse.current != null)
                {
                    playerController.LookInput = Mouse.current.delta.ReadValue();
                    
                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        playerController.AttackRequested = true;
                    }
                }

                if (Keyboard.current != null)
                {
                    if (Keyboard.current.digit1Key.wasPressedThisFrame) playerController.ActiveSlotChange = 0;
                    if (Keyboard.current.digit2Key.wasPressedThisFrame) playerController.ActiveSlotChange = 1;
                    if (Keyboard.current.digit3Key.wasPressedThisFrame) playerController.ActiveSlotChange = 2;
                    if (Keyboard.current.qKey.wasPressedThisFrame) playerController.AbilityRequested = true;
                    if (Keyboard.current.eKey.wasPressedThisFrame) playerController.InteractRequested = true;
                }
            }
        }
    }
}
