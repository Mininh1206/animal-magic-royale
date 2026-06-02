using UnityEngine;
using AnimalMagicRoyale.Core;
using System.Collections;

namespace AnimalMagicRoyale.Components.UI
{
    /// <summary>
    /// Controlador central del HUD in-game. Gestiona la visibilidad global usando UI Toolkit.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UIElements.UIDocument))]
    [RequireComponent(typeof(HealthBarUI))]
    [RequireComponent(typeof(SpellInventoryUI))]
    [RequireComponent(typeof(AbilityUI))]
    [RequireComponent(typeof(KillFeedUI))]
    [RequireComponent(typeof(ZoneTimerUI))]
    [RequireComponent(typeof(MinimapUI))]
    [RequireComponent(typeof(PlayerCountUI))]
    [RequireComponent(typeof(TeamUI))]
    [RequireComponent(typeof(DamageNumberUI))]
    [RequireComponent(typeof(InteractionUI))]
    [RequireComponent(typeof(InventoryUI))]
    public class HUDManager : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private GameStateEvent onGameStateChanged;

        private HealthBarUI healthBarUI;
        private SpellInventoryUI spellInventoryUI;
        private AbilityUI abilityUI;
        private KillFeedUI killFeedUI;
        private ZoneTimerUI zoneTimerUI;
        private MinimapUI minimapUI;
        private PlayerCountUI playerCountUI;
        private TeamUI teamUI;
        private DamageNumberUI damageNumberUI;
        private InteractionUI interactionUI;
        private InventoryUI inventoryUI;
        
        private UnityEngine.UIElements.UIDocument uiDocument;

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
            uiDocument = GetComponent<UnityEngine.UIElements.UIDocument>();
            
            healthBarUI = GetComponent<HealthBarUI>();
            spellInventoryUI = GetComponent<SpellInventoryUI>();
            abilityUI = GetComponent<AbilityUI>();
            killFeedUI = GetComponent<KillFeedUI>();
            zoneTimerUI = GetComponent<ZoneTimerUI>();
            minimapUI = GetComponent<MinimapUI>();
            playerCountUI = GetComponent<PlayerCountUI>();
            teamUI = GetComponent<TeamUI>();
            damageNumberUI = GetComponent<DamageNumberUI>();
            interactionUI = GetComponent<InteractionUI>();
            
            // Try to get or add InventoryUI
            inventoryUI = GetComponent<InventoryUI>();
            if (inventoryUI == null) inventoryUI = gameObject.AddComponent<InventoryUI>();
            
            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                var root = uiDocument.rootVisualElement;
                if (healthBarUI != null) healthBarUI.Initialize(root);
                if (spellInventoryUI != null) spellInventoryUI.Initialize(root);
                if (abilityUI != null) abilityUI.Initialize(root);
                if (killFeedUI != null) killFeedUI.Initialize(root);
                if (zoneTimerUI != null) zoneTimerUI.Initialize(root);
                if (minimapUI != null) minimapUI.Initialize(root);
                if (playerCountUI != null) playerCountUI.Initialize(root);
                if (teamUI != null) teamUI.Initialize(root);
                if (damageNumberUI != null) damageNumberUI.Initialize(root);
                if (interactionUI != null) interactionUI.Initialize(root);
                if (inventoryUI != null) inventoryUI.Initialize(root);
            }

            // Debug.Log("[HUDManager] Start executed (UI Toolkit).");
            if (GameManager.Instance != null && GameManager.Instance.StateMachine.CurrentState is WaitingState)
            {
                SetHUDActive(false);
            }
            else
            {
                SetHUDActive(true);
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            // Debug.Log($"[HUDManager] GameState changed to {state}");
            switch (state)
            {
                case GameState.Waiting:
                    SetHUDActive(true);
                    break;
                case GameState.Playing:
                    SetHUDActive(true);

                    if (healthBarUI != null || spellInventoryUI != null || abilityUI != null)
                    {
                        var player = FindAnyObjectByType<AnimalMagicRoyale.Player.PlayerController>();
                        if (player != null)
                        {
                            if (healthBarUI != null) healthBarUI.SetTrackedPlayer(player.gameObject);
                            if (spellInventoryUI != null) spellInventoryUI.SetTrackedInventory(player.GetComponent<AnimalMagicRoyale.Components.SpellInventory>());
                            if (inventoryUI != null) inventoryUI.SetTrackedInventory(player.GetComponent<AnimalMagicRoyale.Components.SpellInventory>());
                            
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
                            if (teamUI != null) StartCoroutine(SetupTeamDelayed(player.gameObject));
                            
                            // Debug.Log($"[HUDManager] Tracked player assigned: {player.gameObject.name}");
                        }
                        else
                        {
                            Debug.LogWarning("[HUDManager] Could not find PlayerController to assign to HUD.");
                        }
                    }
                    break;
                case GameState.GameOver:
                    SetHUDActive(false);
                    break;
            }
        }
        
        private void SetHUDActive(bool isActive)
        {
            // Debug.Log($"[HUDManager] Setting HUD active state to: {isActive}");
            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.style.display = isActive ? UnityEngine.UIElements.DisplayStyle.Flex : UnityEngine.UIElements.DisplayStyle.None;
            }
        }

        private IEnumerator SetupTeamDelayed(GameObject player)
        {
            yield return new WaitForSeconds(0.5f);
            if (teamUI != null && player != null)
            {
                teamUI.SetupTeam(player);
            }
        }
    }
}
