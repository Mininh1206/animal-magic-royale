using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnimalMagicRoyale.Components.UI
{
    public class MinimapUI : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera minimapCamera;
        [SerializeField] private Transform trackedTarget;
        [SerializeField] private RenderTexture minimapRenderTexture;
        
        private VisualElement minimapImage;
        
        public void Initialize(VisualElement root)
        {
            minimapImage = root.Q<VisualElement>("minimap-image");
            if (minimapImage != null && minimapRenderTexture != null)
            {
                minimapImage.style.backgroundImage = new StyleBackground(
                    Background.FromRenderTexture(minimapRenderTexture));
            }
        }

        private void LateUpdate()
        {
            if (minimapCamera != null && trackedTarget != null)
            {
                // Follow target X/Z but keep fixed height
                Vector3 newPos = trackedTarget.position;
                newPos.y = minimapCamera.transform.position.y;
                minimapCamera.transform.position = newPos;
            }
        }
        
        public void SetTrackedTarget(Transform target)
        {
            trackedTarget = target;
        }

        private System.Collections.Generic.Dictionary<GameObject, VisualElement> teamMarkers = new System.Collections.Generic.Dictionary<GameObject, VisualElement>();
        
        public static bool RevealEnemiesOnMinimap = false;

        private void Update()
        {
            if (trackedTarget == null || minimapImage == null || AnimalMagicRoyale.Core.GameManager.Instance == null || AnimalMagicRoyale.Core.TeamManager.Instance == null) return;
            
            float orthographicSize = minimapCamera != null ? minimapCamera.orthographicSize : 20f;
            int myTeam = AnimalMagicRoyale.Core.TeamManager.Instance.GetTeam(trackedTarget.gameObject);
            if (myTeam == -1) return;

            float minimapRadius = minimapImage.resolvedStyle.width / 2f;
            if (minimapRadius <= 0) minimapRadius = 80f; // Fallback

            foreach (var player in AnimalMagicRoyale.Core.GameManager.Instance.AlivePlayers)
            {
                if (player == trackedTarget.gameObject) continue;

                int pTeam = AnimalMagicRoyale.Core.TeamManager.Instance.GetTeam(player);
                bool isEnemy = pTeam != myTeam;

                if (isEnemy && !RevealEnemiesOnMinimap)
                {
                    if (teamMarkers.TryGetValue(player, out VisualElement hiddenMarker))
                    {
                        hiddenMarker.style.display = DisplayStyle.None;
                    }
                    continue;
                }

                if (!teamMarkers.TryGetValue(player, out VisualElement marker))
                {
                    marker = new VisualElement();
                    marker.style.position = Position.Absolute;
                    marker.style.width = 12;
                    marker.style.height = 12;
                    marker.style.borderTopLeftRadius = 6;
                    marker.style.borderTopRightRadius = 6;
                    marker.style.borderBottomLeftRadius = 6;
                    marker.style.borderBottomRightRadius = 6;
                    marker.style.borderTopWidth = 1;
                    marker.style.borderBottomWidth = 1;
                    marker.style.borderLeftWidth = 1;
                    marker.style.borderRightWidth = 1;
                    marker.style.borderTopColor = Color.black;
                    marker.style.borderBottomColor = Color.black;
                    marker.style.borderLeftColor = Color.black;
                    marker.style.borderRightColor = Color.black;
                    
                    minimapImage.Add(marker);
                    teamMarkers[player] = marker;
                }

                marker.style.backgroundColor = isEnemy ? new Color(1f, 0.2f, 0.2f) : Color.green;

                Vector3 viewportPos = minimapCamera.WorldToViewportPoint(player.transform.position);
                
                // Viewport is (0,0) at bottom-left and (1,1) at top-right
                // UI is (0,0) at top-left
                float uiX = (viewportPos.x * minimapImage.resolvedStyle.width) - 6f; // -6 for center pivot
                float uiY = ((1f - viewportPos.y) * minimapImage.resolvedStyle.height) - 6f;

                // Check distance from center (using normalized coordinates relative to center 0.5, 0.5)
                float normX = (viewportPos.x - 0.5f) * 2f;
                float normY = (viewportPos.y - 0.5f) * 2f;
                float distFromCenter = Mathf.Sqrt(normX * normX + normY * normY);

                if (distFromCenter > 1f)
                {
                    marker.style.display = DisplayStyle.None;
                }
                else
                {
                    marker.style.display = DisplayStyle.Flex;
                    marker.style.left = uiX;
                    marker.style.top = uiY;
                }
            }

            // Cleanup dead/removed teammates
            var toRemove = new System.Collections.Generic.List<GameObject>();
            foreach (var kvp in teamMarkers)
            {
                if (!AnimalMagicRoyale.Core.GameManager.Instance.AlivePlayers.Contains(kvp.Key))
                {
                    kvp.Value.RemoveFromHierarchy();
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var k in toRemove) teamMarkers.Remove(k);
        }
    }
}
