using UnityEngine;
using UnityEngine.UI;

namespace AnimalMagicRoyale.Components.UI
{
    public class MinimapUI : MonoBehaviour
    {
        [SerializeField] private RawImage minimapImage;
        [SerializeField] private UnityEngine.Camera minimapCamera;
        [SerializeField] private Transform trackedTarget;
        
        // This script serves as a placeholder to be expanded when the minimap RenderTexture is created.
        
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
