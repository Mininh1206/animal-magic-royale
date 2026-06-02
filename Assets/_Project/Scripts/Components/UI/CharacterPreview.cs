using UnityEngine;
using UnityEngine.UIElements;
using AnimalMagicRoyale.Core.Data;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Components.UI
{
    public class CharacterPreview : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private UnityEngine.Camera previewCamera;
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private Transform previewSpawnPoint;
        
        [Header("Interaction")]
        [SerializeField] private float rotationSpeed = 200f;

        private GameObject currentPreviewModel;
        private SkinManager skinManager;
        private bool isDragging = false;
        private Vector2 lastMousePos;

        private void Awake()
        {
            if (previewSpawnPoint != null)
            {
                // Assign a temporary skin manager to handle the FBX swap logic 
                // directly on the spawn point, avoiding rewriting the swap code.
                skinManager = previewSpawnPoint.gameObject.AddComponent<SkinManager>();
                
                // Let's bind an Animator via code or ignore anims if we just want a T-pose
                // Wait, CharacterAnimationHandler is required by SkinManager
                var animHandler = previewSpawnPoint.gameObject.AddComponent<CharacterAnimationHandler>();
            }
        }



        public void BindToUI(VisualElement targetContainer)
        {
            if (targetContainer != null && renderTexture != null)
            {
                // Assign the render texture as the background image of the UI element
                targetContainer.style.backgroundImage = Background.FromRenderTexture(renderTexture);

                // Setup drag rotation
                targetContainer.RegisterCallback<PointerDownEvent>(evt =>
                {
                    isDragging = true;
                    lastMousePos = evt.position;
                    targetContainer.CapturePointer(evt.pointerId);
                });

                targetContainer.RegisterCallback<PointerMoveEvent>(evt =>
                {
                    if (isDragging)
                    {
                        Vector2 delta = (Vector2)evt.position - lastMousePos;
                        RotateModel(-delta.x);
                        lastMousePos = evt.position;
                    }
                });

                targetContainer.RegisterCallback<PointerUpEvent>(evt =>
                {
                    isDragging = false;
                    targetContainer.ReleasePointer(evt.pointerId);
                });
            }
        }

        private void RotateModel(float deltaX)
        {
            if (previewSpawnPoint != null)
            {
                previewSpawnPoint.Rotate(Vector3.up, deltaX * rotationSpeed * Time.deltaTime, Space.World);
            }
        }

        public void ShowPreview(SkinData skin, AnimalType animalType = null)
        {
            if (skinManager != null && skin != null)
            {
                skinManager.ApplySkin(skin, animalType);
                currentPreviewModel = skinManager.gameObject; 
                
                // Force rotation to -210 on Y as requested
                if (previewSpawnPoint != null)
                {
                    previewSpawnPoint.localRotation = Quaternion.Euler(0, -210, 0);
                }

                // Force a render in case the camera was disabled or sleeping
                if (previewCamera != null && !previewCamera.enabled)
                {
                    previewCamera.enabled = true;
                }
            }
        }

        public void ClearPreview()
        {
            if (previewSpawnPoint != null)
            {
                foreach(Transform child in previewSpawnPoint)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
