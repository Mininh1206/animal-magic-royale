using System.Collections.Generic;
using UnityEngine;
using AnimalMagicRoyale.Spells;

namespace AnimalMagicRoyale.Core
{
    public class ProjectilePoolManager : MonoBehaviour
    {
        public static ProjectilePoolManager Instance { get; private set; }

        private Dictionary<GameObject, ObjectPool<Projectile>> pools = new Dictionary<GameObject, ObjectPool<Projectile>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public Projectile GetProjectile(GameObject prefab)
        {
            if (prefab == null) return null;

            if (!pools.ContainsKey(prefab))
            {
                GameObject poolContainer = new GameObject($"Pool_{prefab.name}");
                poolContainer.transform.SetParent(transform);

                Projectile projComp = prefab.GetComponent<Projectile>();
                if (projComp == null)
                {
                    Debug.LogError($"[ProjectilePoolManager] Prefab {prefab.name} missing Projectile component.");
                    return null;
                }

                pools[prefab] = new ObjectPool<Projectile>(projComp, 10, poolContainer.transform);
            }

            return pools[prefab].Get();
        }

        public void ReleaseProjectile(GameObject prefab, Projectile instance)
        {
            if (prefab != null && pools.ContainsKey(prefab))
            {
                pools[prefab].Release(instance);
            }
            else
            {
                Destroy(instance.gameObject);
            }
        }
    }
}
