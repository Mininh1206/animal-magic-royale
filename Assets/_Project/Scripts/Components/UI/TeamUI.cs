using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Components.UI
{
    public class TeamUI : MonoBehaviour
    {
        private VisualElement teamContainer;
        private GameObject localPlayer;
        private List<GameObject> teammates = new List<GameObject>();
        private Dictionary<GameObject, VisualElement> teammateVisuals = new Dictionary<GameObject, VisualElement>();

        public void Initialize(VisualElement root)
        {
            teamContainer = root.Q<VisualElement>("team-ui-container");
            if (teamContainer != null)
            {
                teamContainer.Clear();
                teamContainer.style.visibility = Visibility.Hidden;
            }
        }

        public void SetupTeam(GameObject player)
        {
            localPlayer = player;
            teammates.Clear();
            teammateVisuals.Clear();
            if (teamContainer != null) teamContainer.Clear();

            if (TeamManager.Instance == null || GameManager.Instance == null || localPlayer == null) 
            {
                Debug.LogWarning("[TeamUI] Cannot setup team: missing managers or local player is null.");
                return;
            }

            int myTeam = TeamManager.Instance.GetTeam(localPlayer);
            Debug.Log($"[TeamUI] SetupTeam called for localPlayer {localPlayer.name}. Assigned teamId: {myTeam}. Total AlivePlayers: {GameManager.Instance.AlivePlayersCount}");
            
            if (myTeam == -1) return;

            foreach (var p in GameManager.Instance.AlivePlayers)
            {
                if (p != localPlayer && TeamManager.Instance.GetTeam(p) == myTeam)
                {
                    teammates.Add(p);
                    CreateTeammateVisual(p);
                    
                    var health = p.GetComponent<HealthComponent>();
                    if (health != null && health.onHealthChanged != null)
                    {
                        health.onHealthChanged.RegisterListener(HandleTeammateHealth);
                    }
                }
            }

            // Hide container if no teammates
            if (teamContainer != null)
            {
                teamContainer.style.visibility = teammates.Count > 0 ? Visibility.Visible : Visibility.Hidden;
            }
            
            Debug.Log($"[TeamUI] Found {teammates.Count} teammates for teamId {myTeam}. Team container visibility set to {(teammates.Count > 0 ? "Visible" : "Hidden")}");
        }

        private void CreateTeammateVisual(GameObject teammate)
        {
            var memberRow = new VisualElement();
            memberRow.AddToClassList("team-member");

            var nameLabel = new Label(teammate.name);
            nameLabel.AddToClassList("team-member-name");

            var hpLabel = new Label("100%");
            hpLabel.AddToClassList("team-member-hp-text");

            memberRow.Add(nameLabel);
            memberRow.Add(hpLabel);

            var hpBg = new VisualElement();
            hpBg.AddToClassList("team-member-hp-bg");

            var hpFill = new VisualElement();
            hpFill.name = "hp-fill";
            hpFill.AddToClassList("team-member-hp-fill");
            hpBg.Add(hpFill);

            var container = new VisualElement();
            container.Add(memberRow);
            container.Add(hpBg);

            teamContainer.Add(container);
            teammateVisuals.Add(teammate, container);

            // Set initial health
            UpdateVisual(teammate);
        }

        private void HandleTeammateHealth(HealthChangedPayload payload)
        {
            if (payload.target != null && teammateVisuals.ContainsKey(payload.target))
            {
                UpdateVisual(payload.target);
            }
        }

        private void UpdateVisual(GameObject teammate)
        {
            var health = teammate.GetComponent<HealthComponent>();
            if (health != null && teammateVisuals.TryGetValue(teammate, out var visual))
            {
                float perc = health.CurrentHealth / health.maxHealth;
                
                var hpLabel = visual.Q<Label>(null, "team-member-hp-text");
                if (hpLabel != null) hpLabel.text = $"{(int)(perc * 100)}%";
                
                var hpFill = visual.Q<VisualElement>("hp-fill");
                if (hpFill != null) hpFill.style.width = Length.Percent(perc * 100f);

                if (!health.IsAlive)
                {
                    hpLabel.text = "ELIMINATED";
                    hpLabel.style.color = new StyleColor(Color.red);
                    visual.style.opacity = 0.5f;
                }
            }
        }

        private void OnDisable()
        {
            foreach (var t in teammates)
            {
                if (t != null)
                {
                    var health = t.GetComponent<HealthComponent>();
                    if (health != null && health.onHealthChanged != null)
                    {
                        health.onHealthChanged.UnregisterListener(HandleTeammateHealth);
                    }
                }
            }
        }

        private Dictionary<GameObject, Label> floatingNames = new Dictionary<GameObject, Label>();
        public static bool RevealEnemiesOnScreen = false;

        private void Update()
        {
            if (UnityEngine.Camera.main == null || teamContainer == null || teamContainer.panel == null) return;
            var root = teamContainer.parent;
            if (root == null) return;
            if (GameManager.Instance == null || TeamManager.Instance == null || localPlayer == null) return;

            int myTeam = TeamManager.Instance.GetTeam(localPlayer);
            if (myTeam == -1) return;

            foreach (var p in GameManager.Instance.AlivePlayers)
            {
                if (p == null || p == localPlayer) continue;
                
                int pTeam = TeamManager.Instance.GetTeam(p);
                bool isEnemy = pTeam != myTeam;
                
                if (isEnemy && !RevealEnemiesOnScreen)
                {
                    // If it's an enemy and wallhack is not active, ensure we hide their label if it exists
                    if (floatingNames.TryGetValue(p, out Label hiddenLabel))
                    {
                        hiddenLabel.style.display = DisplayStyle.None;
                    }
                    continue;
                }
                
                if (!floatingNames.TryGetValue(p, out Label nameLabel))
                {
                    nameLabel = new Label(p.name);
                    nameLabel.style.position = Position.Absolute;
                    nameLabel.style.fontSize = 16;
                    nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    nameLabel.style.textShadow = new TextShadow { color = Color.black, offset = new Vector2(1, 1), blurRadius = 1f };
                    root.Add(nameLabel);
                    floatingNames[p] = nameLabel;
                }

                nameLabel.style.color = isEnemy ? new Color(1f, 0.2f, 0.2f) : new Color(0.2f, 1f, 0.2f); // Red for enemy, Light green for ally

                // Check visibility
                var health = p.GetComponent<HealthComponent>();
                if (health != null && !health.IsAlive)
                {
                    nameLabel.style.display = DisplayStyle.None;
                    continue;
                }

                // Calcular el offset usando los Renderers para obtener la altura visual real
                float topY = p.transform.position.y + 2.5f; // Fallback
                var renderers = p.GetComponentsInChildren<Renderer>();
                if (renderers != null && renderers.Length > 0)
                {
                    float maxY = renderers[0].bounds.max.y;
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        if (renderers[i].bounds.max.y > maxY)
                        {
                            maxY = renderers[i].bounds.max.y;
                        }
                    }
                    topY = maxY + 0.5f; // Ajustado exactamente encima del modelo visual
                }

                Vector3 targetPos = new Vector3(p.transform.position.x, topY, p.transform.position.z);
                Vector3 screenPos = UnityEngine.Camera.main.WorldToScreenPoint(targetPos);

                // Behind camera
                if (screenPos.z < 0)
                {
                    nameLabel.style.display = DisplayStyle.None;
                    continue;
                }

                // Raycast occlusion check (only if it's an ally, enemies with wallhack skip this)
                if (!isEnemy)
                {
                    Vector3 camPos = UnityEngine.Camera.main.transform.position;
                    Vector3 dir = (targetPos - Vector3.up * 1f) - camPos;
                    if (Physics.Raycast(camPos, dir.normalized, out RaycastHit hit, dir.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider.gameObject != p && !hit.collider.transform.IsChildOf(p.transform))
                        {
                            nameLabel.style.display = DisplayStyle.None;
                            continue;
                        }
                    }
                }

                nameLabel.style.display = DisplayStyle.Flex;

                Vector2 panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(teamContainer.panel, targetPos, UnityEngine.Camera.main);

                // Center the label text
                nameLabel.style.left = panelPos.x - 50; // Approximated width / 2
                nameLabel.style.top = panelPos.y - 10; // offset
                nameLabel.style.width = 100;
                nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            }
        }
    }
}
