using UnityEngine;
using AnimalMagicRoyale.Spells.Effects;

namespace AnimalMagicRoyale.Spells
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        private SpellData spellData;
        private GameObject caster;
        private Rigidbody rb;
        private float lifetime = 5f;
        private float spawnTime;
        private bool isInitialized = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            var col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        public void Initialize(SpellData data, GameObject caster, Vector3 direction)
        {
            this.spellData = data;
            this.caster = caster;
            this.spawnTime = Time.time;
            
            rb.linearVelocity = direction.normalized * data.projectileSpeed;
            
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized) return;

            if (Time.time - spawnTime >= lifetime)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isInitialized) return;
            if (other.gameObject == caster) return;
            
            // Ignorar choques entre otros proyectiles a menos que sea para reaccionar
            var otherProj = other.GetComponent<Projectile>();
            if (otherProj != null)
            {
                OnProjectileClash(otherProj);
                return;
            }

            // Ignorar el colisionador visual de la zona
            if (AnimalMagicRoyale.Core.ZoneManager.Instance != null && 
                other.transform == AnimalMagicRoyale.Core.ZoneManager.Instance.zoneVisual)
            {
                return;
            }

            // Debug para saber contra qué choca
            Debug.Log($"[Projectile] Chocó contra: {other.gameObject.name}");

            if (spellData != null && spellData.effects != null)
            {
                foreach (var effect in spellData.effects)
                {
                    if (effect != null)
                    {
                        effect.Apply(caster, other.gameObject);
                    }
                }
            }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            isInitialized = false;
            rb.linearVelocity = Vector3.zero;
            AnimalMagicRoyale.Core.ProjectilePoolManager.Instance.ReleaseProjectile(spellData.projectilePrefab, this);
        }

        protected virtual void OnProjectileClash(Projectile other)
        {
            Debug.Log($"[Projectile] Choque mágico detectado entre {gameObject.name} y {other.gameObject.name}");
            
            // TODO: Futuro: Aquí se puede comprobar si spellData es Fuego y el otro es Agua,
            // instanciar un VFX de explosión o humo en el punto medio, etc.
            
            // Por defecto, ambos proyectiles se anulan y vuelven al pool
            ReturnToPool();
        }
    }
}
