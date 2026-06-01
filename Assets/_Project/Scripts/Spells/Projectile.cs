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

        private int bouncesLeft;
        private GameObject homingTarget;

        public void Initialize(SpellData data, GameObject caster, Vector3 direction)
        {
            this.spellData = data;
            this.caster = caster;
            this.spawnTime = Time.time;
            this.bouncesLeft = data.maxBounces;
            this.homingTarget = null;
            
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

        private void FixedUpdate()
        {
            if (!isInitialized) return;

            if (spellData != null && spellData.isHoming)
            {
                if (homingTarget == null) FindHomingTarget();

                if (homingTarget != null)
                {
                    Vector3 direction = (homingTarget.transform.position + Vector3.up * 0.5f - transform.position).normalized;
                    Quaternion lookRot = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.fixedDeltaTime * 10f);
                    rb.linearVelocity = transform.forward * spellData.projectileSpeed;
                }
            }
        }

        private void FindHomingTarget()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 15f);
            float closestDist = float.MaxValue;
            int casterTeam = AnimalMagicRoyale.Core.TeamManager.Instance != null && caster != null ? AnimalMagicRoyale.Core.TeamManager.Instance.GetTeam(caster) : -1;

            foreach(var hit in hits)
            {
                var hc = hit.GetComponentInParent<AnimalMagicRoyale.Components.HealthComponent>();
                if (hc != null && hc.gameObject != caster)
                {
                    if (casterTeam != -1 && AnimalMagicRoyale.Core.TeamManager.Instance != null && 
                        AnimalMagicRoyale.Core.TeamManager.Instance.GetTeam(hc.gameObject) == casterTeam)
                    {
                        continue;
                    }

                    float dist = Vector3.Distance(transform.position, hc.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        homingTarget = hc.gameObject;
                    }
                }
            }

            if (homingTarget != null)
            {
                Debug.Log($"[Projectile Homing] {gameObject.name} fijó objetivo en {homingTarget.name}");
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
                // Ignorar colisiones entre proyectiles del mismo lanzador
                if (otherProj.caster == this.caster) return;
                
                // Ignorar colisiones entre proyectiles del mismo equipo
                if (AnimalMagicRoyale.Core.TeamManager.Instance != null && caster != null && otherProj.caster != null)
                {
                    int teamA = AnimalMagicRoyale.Core.TeamManager.Instance.GetTeam(this.caster);
                    int teamB = AnimalMagicRoyale.Core.TeamManager.Instance.GetTeam(otherProj.caster);
                    if (teamA != -1 && teamA == teamB) return;
                }

                OnProjectileClash(otherProj);
                return;
            }

            // Ignorar el colisionador visual de la zona
            if (AnimalMagicRoyale.Core.ZoneManager.Instance != null && 
                other.transform == AnimalMagicRoyale.Core.ZoneManager.Instance.zoneVisual)
            {
                return;
            }

            // Rebotes (si no chocó contra alguien vivo)
            if (targetHealth == null && bouncesLeft > 0)
            {
                Vector3 normal = -rb.linearVelocity.normalized;
                if (Physics.Raycast(transform.position - rb.linearVelocity.normalized * 0.5f, rb.linearVelocity.normalized, out RaycastHit hit, 2f))
                {
                    if (hit.collider == other) normal = hit.normal;
                }
                
                rb.linearVelocity = Vector3.Reflect(rb.linearVelocity, normal).normalized * spellData.projectileSpeed;
                transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
                bouncesLeft--;
                Debug.Log($"[Projectile Bounce] {gameObject.name} rebotó en {other.gameObject.name}. Rebotes restantes: {bouncesLeft}");
                return;
            }

            // Debug para saber contra qué choca (ignorar si es un rebote)
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
            ReturnToPool();
        }
    }
}
