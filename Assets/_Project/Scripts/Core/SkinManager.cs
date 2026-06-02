using UnityEngine;
using AnimalMagicRoyale.Core.Data;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Core
{
    [RequireComponent(typeof(CharacterAnimationHandler))]
    public class SkinManager : MonoBehaviour
    {
        [Header("Skin Setup")]
        [SerializeField] private Transform modelParent; // Default spawn point for the FBX model
        [SerializeField] private Transform firePointParent; // Where fire point should attach to dynamically if needed

        private GameObject _currentModelInstance;
        private SkinData _currentSkin;
        private CharacterAnimationHandler _animHandler;
        private SpellInventory _inventory; // Optional: If we need to reassign FirePoint

        public SkinData CurrentSkin => _currentSkin;

        private void Awake()
        {
            _animHandler = GetComponent<CharacterAnimationHandler>();
            _inventory = GetComponent<SpellInventory>();
            
            if (modelParent == null)
            {
                // Try to find an existing model container
                modelParent = transform.Find("ModelContainer");
                
                // If not found, use this transform, but warn unless it's a known preview/spawn setup
                if (modelParent == null)
                {
                    bool isPreview = gameObject.name.Contains("SpawnPoint") || gameObject.name.Contains("Preview");
                    if (!isPreview)
                    {
                        Debug.LogWarning($"[SkinManager] modelParent is null on {gameObject.name}. Models will be spawned directly under it, which may cause transform issues.");
                    }
                    modelParent = transform;
                }
            }
        }

        private void Start()
        {
            // Note: In actual gameplay, GameManager or PlayerSpawner should call ApplySkin(PlayerSetupData.SelectedSkin)
            // If none was applied, we might fall back to a default, but we'll leave that to the spawner logic.
        }

        public void ApplyDefault(AnimalType animal)
        {
            if (animal == null || animal.defaultSkin == null)
            {
                Debug.LogWarning("[SkinManager] Cannot apply default skin. Animal or defaultSkin is null.");
                return;
            }
            ApplySkin(animal.defaultSkin, animal);
        }

        public void ApplySkin(SkinData skin, AnimalType animalType = null)
        {
            if (skin == null || skin.modelPrefab == null)
            {
                Debug.LogError("[SkinManager] Attempted to apply null skin or skin with null modelPrefab.");
                return;
            }

            ClearPreviousModel();
            InstantiateModel(skin);
            UpdateAnimator(skin);
            
            Bounds modelBounds = CalculateModelBounds(_currentModelInstance);
            
            SetupFirePoint(modelBounds);
            UpdateCollisionComponents(modelBounds);

            // Assign audio data
            var sfxHandler = GetComponent<CharacterSFXHandler>();
            if (sfxHandler != null && animalType != null && animalType.audioData != null)
            {
                sfxHandler.SetAudioData(animalType.audioData);
            }

            _currentSkin = skin;
            // Debug.Log($"[SkinManager] Applied skin: {skin.skinName} on {gameObject.name}");
        }

        private void ClearPreviousModel()
        {
            if (_currentModelInstance != null)
            {
                Destroy(_currentModelInstance);
            }
            else
            {
                foreach (Transform child in modelParent)
                {
                    if (child != firePointParent)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        private void InstantiateModel(SkinData skin)
        {
            _currentModelInstance = Instantiate(skin.modelPrefab, modelParent);
            _currentModelInstance.transform.localPosition = Vector3.zero;
            _currentModelInstance.transform.localRotation = Quaternion.identity;
            _currentModelInstance.transform.localScale = Vector3.one;
            SetLayerRecursively(_currentModelInstance, gameObject.layer);
        }

        private void UpdateAnimator(SkinData skin)
        {
            Animator newAnimator = _currentModelInstance.GetComponentInChildren<Animator>();
            if (newAnimator != null)
            {
                if (skin != null && skin.animatorController != null)
                {
                    newAnimator.runtimeAnimatorController = skin.animatorController;
                    // Debug.Log($"[SkinManager] Applied AnimatorController {skin.animatorController.name}");
                }

                if (_animHandler != null)
                {
                    _animHandler.SetAnimator(newAnimator);
                    // Debug.Log($"[SkinManager] Re-bound Animator for {_currentModelInstance.name}");
                }
            }
        }

        private Bounds CalculateModelBounds(GameObject modelInstance)
        {
            var renderers = modelInstance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(modelInstance.transform.position, Vector3.one);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }

        private void SetupFirePoint(Bounds bounds)
        {
            Transform newFirePoint = _currentModelInstance.transform.Find("FirePoint");
            if (newFirePoint == null)
            {
                GameObject fpObj = new GameObject("FirePoint");
                newFirePoint = fpObj.transform;
                newFirePoint.SetParent(_currentModelInstance.transform, false);

                // Posicionar FirePoint en la parte frontal-alta del animal
                Vector3 localCenter = _currentModelInstance.transform.InverseTransformPoint(bounds.center);
                float chestHeight = bounds.size.y * 0.7f;
                float forwardOffset = bounds.extents.z * 1.1f;
                newFirePoint.localPosition = localCenter + new Vector3(0, chestHeight, forwardOffset);
                
                // Debug.Log($"[SkinManager] Auto-generated FirePoint for {_currentModelInstance.name} at {newFirePoint.localPosition}");
            }

            if (_inventory != null)
            {
                _inventory.SetFirePoint(newFirePoint);
                // Debug.Log($"[SkinManager] Re-bound FirePoint for {_currentModelInstance.name}");
            }
        }

        private void UpdateCollisionComponents(Bounds bounds)
        {
            float height = bounds.size.y;
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.z) * 1.1f;
            Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
            localCenter.x = 0;
            localCenter.z = 0;

            // Apply to NavMeshAgent (for AI logic)
            var navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.height = height;
                navAgent.radius = radius;
                // Debug.Log($"[SkinManager] Adjusted NavMeshAgent. Height: {height:F2}, Radius: {radius:F2}");
            }

            // Apply to CharacterController (for Player logic)
            var charController = GetComponent<CharacterController>();
            if (charController != null)
            {
                charController.height = height;
                charController.radius = radius;
                charController.center = localCenter;
                // Debug.Log($"[SkinManager] Adjusted CharacterController. Height: {height:F2}, Radius: {radius:F2}");
            }

            // Añadir BoxCollider al hijo para que bots y proyectiles colisionen con él (NavMeshAgent NO colisiona)
            BoxCollider boxCollider = _currentModelInstance.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = _currentModelInstance.AddComponent<BoxCollider>();
            }
            
            // Adjust box size exactly to the bounds, local to the model instance
            Vector3 childLocalCenter = _currentModelInstance.transform.InverseTransformPoint(bounds.center);
            boxCollider.center = childLocalCenter;
            boxCollider.size = bounds.size;
            boxCollider.isTrigger = false; // Queremos que reciba los rayos y overlaps físicos
            
            // Debug.Log($"[SkinManager] Added/Adjusted BoxCollider on model {_currentModelInstance.name}. Size: {boxCollider.size}");
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                if (child != null)
                {
                    SetLayerRecursively(child.gameObject, newLayer);
                }
            }
        }

        public void SetVisibility(bool isVisible)
        {
            if (_currentModelInstance != null)
            {
                var renderers = _currentModelInstance.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    // No ocultar particulas u otros efectos críticos si los hay, pero típicamente SkinnedMeshRenderer y MeshRenderer sí.
                    if (r is SkinnedMeshRenderer || r is MeshRenderer)
                    {
                        r.enabled = isVisible;
                    }
                }
                // Debug.Log($"[SkinManager] Visibilidad ajustada a {isVisible} en {_currentModelInstance.name}");
            }
        }
    }
}
