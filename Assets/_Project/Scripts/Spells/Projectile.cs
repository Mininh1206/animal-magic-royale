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
    }
}
