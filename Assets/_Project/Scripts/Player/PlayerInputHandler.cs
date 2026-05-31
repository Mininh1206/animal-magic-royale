using UnityEngine;
using UnityEngine.InputSystem;
using Blocks.Gameplay.Core;
using System.Threading;
using AnimalMagicRoyale.Core;

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
                if (AnimalMagicRoyale.Components.UI.SettingsManager.Instance != null && AnimalMagicRoyale.Components.UI.SettingsManager.Instance.IsOpen) return;
                playerController.MoveInput = context.ReadValue<Vector2>();
            }
        }

        private void HandleJump(InputAction.CallbackContext context)
        {
            if (playerController != null)
            {
                if (AnimalMagicRoyale.Components.UI.SettingsManager.Instance != null && AnimalMagicRoyale.Components.UI.SettingsManager.Instance.IsOpen) return;
                playerController.JumpRequested = true;
            }
        }

        private void HandleSprint(InputAction.CallbackContext context)
        {
            if (playerController != null)
            {
                if (AnimalMagicRoyale.Components.UI.SettingsManager.Instance != null && AnimalMagicRoyale.Components.UI.SettingsManager.Instance.IsOpen) return;
                playerController.IsSprinting = context.ReadValueAsButton();
            }
        }

        private void Update()
        {
            if (playerController != null)
            {
                if (AnimalMagicRoyale.Components.UI.SettingsManager.Instance != null && AnimalMagicRoyale.Components.UI.SettingsManager.Instance.IsOpen)
                {
                    playerController.MoveInput = Vector2.zero;
                    playerController.LookInput = Vector2.zero;
                    playerController.IsSprinting = false;
                    return;
                }
                if (Mouse.current != null)
                {
                    playerController.LookInput = Mouse.current.delta.ReadValue();
                    
                    if (Mouse.current.leftButton.isPressed)
                    {
                        Debug.Log("[PlayerInputHandler] Attack requested via Mouse Left Button.");
                        playerController.AttackRequested = true;
                    }
                }

                if (inputActions.Player.Jump.IsPressed())
                {
                    playerController.JumpRequested = true;
                }

                if (Keyboard.current != null)
                {
                    var kb = AnimalMagicRoyale.Core.KeyBindingManager.Instance;
                    if (kb != null)
                    {
                        if (kb.GetActionDown(AnimalMagicRoyale.Core.KeyBindingManager.GameAction.SelectSlot1)) playerController.ActiveSlotChange = 0;
                        if (kb.GetActionDown(AnimalMagicRoyale.Core.KeyBindingManager.GameAction.SelectSlot2)) playerController.ActiveSlotChange = 1;
                        if (kb.GetActionDown(AnimalMagicRoyale.Core.KeyBindingManager.GameAction.SelectSlot3)) playerController.ActiveSlotChange = 2;
                        
                        if (kb.GetActionDown(AnimalMagicRoyale.Core.KeyBindingManager.GameAction.Ability))
                        {
                            Debug.Log("[PlayerInputHandler] Ability requested via Keyboard.");
                            playerController.AbilityRequested = true;
                        }
                        
                        if (kb.GetActionDown(AnimalMagicRoyale.Core.KeyBindingManager.GameAction.Interact)) playerController.InteractRequested = true;

                        // DEBUG
                        if (kb.GetActionDown(AnimalMagicRoyale.Core.KeyBindingManager.GameAction.LowerHealth))
                        {
                            // Invoca evento de daño sin usar eventbus
                            playerController.TakeDamage(25f, null);
                        }
                        if (kb.GetActionDown(AnimalMagicRoyale.Core.KeyBindingManager.GameAction.HealPlayer))
                        {
                            playerController.Heal(25f);
                        }
                    }
                }
            }
        }
    }
}
