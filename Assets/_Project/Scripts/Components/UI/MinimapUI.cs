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
    }
}
