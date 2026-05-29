using UnityEngine;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Components.UI
{
    /// <summary>
    /// Controlador central del HUD in-game. Gestiona la visibilidad de los
    /// paneles según el estado de la partida y coordina los sub-componentes UI.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject hudContainer; // The main container holding all in-game HUD elements
        [SerializeField] private GameObject healthBarPanel;
        [SerializeField] private GameObject spellInventoryPanel;
        [SerializeField] private GameObject abilityPanel;
        [SerializeField] private GameObject minimapPanel;
        [SerializeField] private GameObject playerCountPanel;
        [SerializeField] private GameObject killFeedPanel;
        [SerializeField] private GameObject zoneTimerPanel;
        
        [Header("Game Over / Lobby")]
        [SerializeField] private GameObject gameOverPanel;

        [Header("Events")]
        [SerializeField] private GameStateEvent onGameStateChanged;

        private void OnEnable()
        {
            if (onGameStateChanged != null)
                onGameStateChanged.RegisterListener(HandleGameStateChanged);
        }

        private void OnDisable()
        {
            if (onGameStateChanged != null)
                onGameStateChanged.UnregisterListener(HandleGameStateChanged);
        }
        
        private void Start()
        {
            // Initial state based on typical GameManager startup (Waiting -> Playing)
            if (GameManager.Instance != null && GameManager.Instance.StateMachine.CurrentState is WaitingState)
            {
                SetHUDActive(false);
            }
            else
            {
                SetHUDActive(true);
            }
            
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Waiting:
                    SetHUDActive(false);
                    if (gameOverPanel != null) gameOverPanel.SetActive(false);
                    break;
                case GameState.Playing:
                    SetHUDActive(true);
                    if (gameOverPanel != null) gameOverPanel.SetActive(false);
                    break;
                case GameState.GameOver:
                    SetHUDActive(false);
                    if (gameOverPanel != null) gameOverPanel.SetActive(true);
                    break;
            }
        }
        
        private void SetHUDActive(bool isActive)
        {
            if (hudContainer != null)
            {
                hudContainer.SetActive(isActive);
            }
            else
            {
                // Fallback si no hay un contenedor global, desactivamos panel por panel
                if (healthBarPanel != null) healthBarPanel.SetActive(isActive);
                if (spellInventoryPanel != null) spellInventoryPanel.SetActive(isActive);
                if (abilityPanel != null) abilityPanel.SetActive(isActive);
                if (minimapPanel != null) minimapPanel.SetActive(isActive);
                if (playerCountPanel != null) playerCountPanel.SetActive(isActive);
                if (killFeedPanel != null) killFeedPanel.SetActive(isActive);
                if (zoneTimerPanel != null) zoneTimerPanel.SetActive(isActive);
            }
        }
    }
}
