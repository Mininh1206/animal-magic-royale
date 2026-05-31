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

        private TrailRenderer trail;
        private Renderer[] childRenderers;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            var col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            trail = GetComponent<TrailRenderer>();
            if (trail == null)
            {
                trail = gameObject.AddComponent<TrailRenderer>();
                trail.time = 0.15f; // Estela corta como un rayo o láser
                trail.startWidth = 0.4f;
                trail.endWidth = 0f;
                trail.autodestruct = false;
                trail.emitting = false;
                
                // Material por defecto compatible con color
                Material defaultMat = new Material(Shader.Find("Sprites/Default"));
                trail.material = defaultMat;
            }

            childRenderers = GetComponentsInChildren<Renderer>();
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

            if (trail != null)
            {
                trail.Clear(); // Limpiar estela de uso previo (Object Pooling)
                trail.emitting = true;
                
                // Asignar el color del hechizo
                Color c = data.spellColor;
                trail.startColor = c;
                c.a = 0f; // Fade out hacia el final de la estela
                trail.endColor = c;
            }

            // Ocultar mallas (MeshRenderer) para que solo se vea la estela/rayo
            if (childRenderers != null)
            {
                foreach (var r in childRenderers)
                {
                    if (r != trail && r != null) 
                    {
                        r.enabled = false;
                    }
                }
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
            if (other.gameObject == caster || other.transform.IsChildOf(caster.transform)) return;
            
            // Check for teammates
            var targetHealth = other.GetComponentInParent<AnimalMagicRoyale.Components.HealthComponent>();
            if (targetHealth != null && targetHealth.gameObject == caster) return; // Failsafe for caster
            
            if (AnimalMagicRoyale.Core.TeamManager.Instance != null && caster != null && targetHealth != null)
            {
                int casterTeam = AnimalMagicRoyale.Core.TeamManager.Instance.GetTeam(caster);
                int targetTeam = AnimalMagicRoyale.Core.TeamManager.Instance.GetTeam(targetHealth.gameObject);
                if (casterTeam == targetTeam) return; // Mismo equipo, ignorar
            }
            
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
            if (trail != null)
            {
                trail.emitting = false;
            }
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
