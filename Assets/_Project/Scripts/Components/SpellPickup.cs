using UnityEngine;
using AnimalMagicRoyale.Spells;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnimalMagicRoyale.Components
{
    public class SpellPickup : MonoBehaviour
    {
        [SerializeField] private float interactionRange = 2f;
        public SpellData containedSpell;
        
        public void Initialize(SpellData spell)
        {
            this.containedSpell = spell;
            
            // Add a visual representation
            GameObject visual = null;
#if UNITY_EDITOR
            // Try to load the scroll book prefab first
            // TODO: Change this path to the new asset you want to use
            string assetPath = "Assets/_Project/Core/Art/ScrollFiles/ScrollBookCandle.fbx";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                visual = Instantiate(prefab, transform);
                visual.transform.localPosition = Vector3.zero;
                // Scale it up
                visual.transform.localScale = Vector3.one * 1.5f; 
                
                // Ocultar luces y elementos que no sean el libro basándonos en la jerarquía exacta
                Transform[] allChildren = visual.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allChildren)
                {
                    string n = t.name;
                    if (n.Contains("directionalLight") || n == "Ground" || 
                        n == "Candle" || n.StartsWith("Feather") || 
                        n == "ink_bottle" || n == "Scroll")
                    {
                        t.gameObject.SetActive(false);
                    }
                }
            }
#endif
            if (visual == null)
            {
                // Fallback to a placeholder cube if fbx is not found or in build (since we can't use AssetDatabase in build)
                // In a real scenario, this would be a prefab reference in a Spawner or Resource.Load
                visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.transform.SetParent(transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = Vector3.one * 0.5f;
                Destroy(visual.GetComponent<Collider>());
                
                var renderer = visual.GetComponent<Renderer>();
                if (renderer != null && spell != null)
                {
                    renderer.material.color = spell.spellColor;
                }
            }
            
            // Make it float and spin
            var bob = gameObject.AddComponent<FloatingEffect>();

            // Add a trigger collider for physical interactions if needed, though we use OverlapSphere
            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = interactionRange;
        }

        public bool TryPickup(GameObject player)
        {
            if (Vector3.Distance(transform.position, player.transform.position) <= interactionRange)
            {
                var inventory = player.GetComponent<SpellInventory>();
                if (inventory != null && containedSpell != null)
                {
                    if (inventory.TryPickupSpell(containedSpell))
                    {
                        Destroy(gameObject);
                        return true;
                    }
                }
            }
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }

    public class FloatingEffect : MonoBehaviour
    {
        private Vector3 startPos;
        public float amplitude = 0.2f;
        public float speed = 2f;
        public float rotationSpeed = 45f;

        void Start()
        {
            startPos = transform.position;
        }

        void Update()
        {
            transform.position = startPos + new Vector3(0.0f, Mathf.Sin(Time.time * speed) * amplitude, 0.0f);
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
