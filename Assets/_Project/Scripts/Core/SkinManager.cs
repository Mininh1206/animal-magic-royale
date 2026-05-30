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
            ApplySkin(animal.defaultSkin);
        }

        public void ApplySkin(SkinData skin)
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

            _currentSkin = skin;
            Debug.Log($"[SkinManager] Applied skin: {skin.skinName} on {gameObject.name}");

            // Re-bind Animator to the CharacterAnimationHandler
            Animator newAnimator = _currentModelInstance.GetComponentInChildren<Animator>();
            if (newAnimator != null && _animHandler != null)
            {
                // Assuming we can rebind or the CharacterAnimationHandler will pick it up
                // We might need to make TargetAnimator public or add a SetAnimator method.
                // Since TargetAnimator has a private setter natively probably, let's use reflection or if we can change CharacterAnimationHandler
                // In CharacterAnimationHandler, TargetAnimator is just a property with get.
                // Actually, let's modify CharacterAnimationHandler to have a SetAnimator method if it doesn't.
                // Or we can just use the Animator component we found and if the handler doesn't support changing it, we need to modify the handler.
                
                var field = typeof(CharacterAnimationHandler).GetField("targetAnimator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if(field != null)
                {
                     field.SetValue(_animHandler, newAnimator);
                     Debug.Log($"[SkinManager] Re-bound Animator for {_currentModelInstance.name}");
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
        }
    }
}
