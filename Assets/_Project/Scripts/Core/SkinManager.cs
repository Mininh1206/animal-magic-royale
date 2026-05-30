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

            // Destroy previous model
            if (_currentModelInstance != null)
            {
                Destroy(_currentModelInstance);
            }
            else
            {
                // Attempt to clean up any initial children of the modelParent to prevent duplicates
                foreach (Transform child in modelParent)
                {
                    // Avoid destroying the FirePoint if it's placed under modelParent but isn't part of a model
                    if (child != firePointParent)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }

            // Instantiate new model
            _currentModelInstance = Instantiate(skin.modelPrefab, modelParent);
            _currentModelInstance.transform.localPosition = Vector3.zero;
            _currentModelInstance.transform.localRotation = Quaternion.identity;
            _currentModelInstance.transform.localScale = Vector3.one;
            
            // Ensure the instantiated model matches the root object's layer so cameras/colliders work correctly
            SetLayerRecursively(_currentModelInstance, gameObject.layer);

            _currentSkin = skin;
            Debug.Log($"[SkinManager] Applied skin: {skin.skinName} on {gameObject.name}");

            // Re-bind Animator to the CharacterAnimationHandler
            Animator newAnimator = _currentModelInstance.GetComponentInChildren<Animator>();
            if (newAnimator != null)
            {
                if (animalType != null && animalType.animatorController != null)
                {
                    newAnimator.runtimeAnimatorController = animalType.animatorController;
                    Debug.Log($"[SkinManager] Applied AnimatorController {animalType.animatorController.name}");
                }

                if (_animHandler != null)
                {
                    var field = typeof(CharacterAnimationHandler).GetField("targetAnimator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if(field != null)
                    {
                         field.SetValue(_animHandler, newAnimator);
                         Debug.Log($"[SkinManager] Re-bound Animator for {_currentModelInstance.name}");
                    }
                }
            }

            // If the model has a specific fire point, we could re-assign it in SpellInventory
            // For now, we assume the SpellInventory FirePoint is independent or we can search for a tag/name
            Transform newFirePoint = _currentModelInstance.transform.Find("FirePoint");
            if (newFirePoint != null && _inventory != null)
            {
                var field = typeof(SpellInventory).GetField("firePoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(_inventory, newFirePoint);
                    Debug.Log($"[SkinManager] Re-bound FirePoint for {_currentModelInstance.name}");
                }
            }

            AdjustPhysicsBounds(_currentModelInstance);
        }

        private void AdjustPhysicsBounds(GameObject modelInstance)
        {
            var renderers = modelInstance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            float height = bounds.size.y;
            // Add a small padding to radius
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.z) * 1.1f;
            Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
            
            // Keep center grounded and centered horizontally
            localCenter.x = 0;
            localCenter.z = 0;
            
            // Apply to CharacterController
            var charController = GetComponent<CharacterController>();
            if (charController != null)
            {
                charController.height = height;
                charController.radius = radius;
                charController.center = localCenter;
                Debug.Log($"[SkinManager] Adjusted CharacterController. Height: {height:F2}, Radius: {radius:F2}");
            }

            // Apply to NavMeshAgent
            var navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.height = height;
                navAgent.radius = radius;
                Debug.Log($"[SkinManager] Adjusted NavMeshAgent. Height: {height:F2}, Radius: {radius:F2}");
            }
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
    }
}
