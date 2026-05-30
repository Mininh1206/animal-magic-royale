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

        private HealthBarUI healthBarUI;
        private SpellInventoryUI spellInventoryUI;
        private AbilityUI abilityUI;

        private void Awake()
        {
            if (onGameStateChanged == null) onGameStateChanged = Resources.Load<GameStateEvent>("Events/GameStateEvent");
        }

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
            if (healthBarPanel != null) healthBarUI = healthBarPanel.GetComponent<HealthBarUI>();
            if (spellInventoryPanel != null) spellInventoryUI = spellInventoryPanel.GetComponent<SpellInventoryUI>();
            if (abilityPanel != null) abilityUI = abilityPanel.GetComponent<AbilityUI>();
            
            // Fallback just in case inspector references are missing
            if (healthBarUI == null) healthBarUI = GetComponentInChildren<HealthBarUI>(true);
            if (spellInventoryUI == null) spellInventoryUI = GetComponentInChildren<SpellInventoryUI>(true);
            if (abilityUI == null) abilityUI = GetComponentInChildren<AbilityUI>(true);
            
            Debug.Log("[HUDManager] Start executed.");
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
            Debug.Log($"[HUDManager] GameState changed to {state}");
            switch (state)
            {
                case GameState.Waiting:
                    SetHUDActive(true);
                    if (gameOverPanel != null) gameOverPanel.SetActive(false);
                    break;
                case GameState.Playing:
                    SetHUDActive(true);
                    if (gameOverPanel != null) gameOverPanel.SetActive(false);

                    if (healthBarUI != null || spellInventoryUI != null || abilityUI != null)
                    {
                        var player = FindAnyObjectByType<AnimalMagicRoyale.Player.PlayerController>();
                        if (player != null)
                        {
                            if (healthBarUI != null) healthBarUI.SetTrackedPlayer(player.gameObject);
                            if (spellInventoryUI != null) spellInventoryUI.SetTrackedInventory(player.GetComponent<AnimalMagicRoyale.Components.SpellInventory>());
                            
                            if (abilityUI != null)
                            {
                                var abilityHolder = player.GetComponent<AnimalMagicRoyale.Components.AbilityHolder>();
                                if (abilityHolder != null && AnimalMagicRoyale.Core.Data.PlayerSetupData.SelectedAnimal != null)
                                {
                                    abilityUI.SetTrackedAbility(abilityHolder, AnimalMagicRoyale.Core.Data.PlayerSetupData.SelectedAnimal.ability);
                                }
                                else if (abilityHolder != null && abilityHolder.Ability != null)
                                {
                                    abilityUI.SetTrackedAbility(abilityHolder, abilityHolder.Ability);
                                }
                            }
                            
                            Debug.Log($"[HUDManager] Tracked player assigned: {player.gameObject.name}");
                        }
                        else
                        {
                            Debug.LogWarning("[HUDManager] Could not find PlayerController to assign to HUD.");
                        }
                    }
                    break;
                case GameState.GameOver:
                    SetHUDActive(false);
                    if (gameOverPanel != null) gameOverPanel.SetActive(true);
                    break;
            }
        }
        
        private void SetHUDActive(bool isActive)
        {
            Debug.Log($"[HUDManager] Setting HUD active state to: {isActive}");
            if (hudContainer != null && hudContainer != this.gameObject)
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
