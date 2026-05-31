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
    }
}
