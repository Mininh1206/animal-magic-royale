using System.Collections.Generic;
using UnityEngine;
using AnimalMagicRoyale.Components;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.AI
{
    public enum TargetType { Enemy, LootBox, ZoneBoundary }

    public struct SensorTarget
    {
        public Transform transform;
        public float distance;
        public TargetType type;

        public SensorTarget(Transform t, float d, TargetType type)
        {
            this.transform = t;
            this.distance = d;
            this.type = type;
        }
    }

    public class AISensorSystem : MonoBehaviour
    {
        [SerializeField] private AISensorConfig config;
        
        public List<SensorTarget> VisibleTargets { get; private set; } = new List<SensorTarget>();
        
        private float lastUpdateTime;
        private Collider[] hearingColliders = new Collider[20]; // Preallocated for NonAlloc

        private void Update()
        {
            if (config == null) return;

            if (Time.time - lastUpdateTime >= config.sensorUpdateInterval)
            {
                UpdateSensor();
                lastUpdateTime = Time.time;
                
                int enemyCount = 0;
                int lootCount = 0;
                foreach(var t in VisibleTargets) {
                    if (t.type == TargetType.Enemy) enemyCount++;
                    if (t.type == TargetType.LootBox) lootCount++;
                }
                Debug.Log($"[AISensorSystem] {gameObject.name}: Scan found {VisibleTargets.Count} targets ({enemyCount} enemies, {lootCount} lootboxes)");
            }
        }

        private void UpdateSensor()
        {
            VisibleTargets.Clear();
            
            // 1. Vision (Raycasts)
            // Ligeramente simplificado: lanzamos rayos en abanico y vemos si golpean un target y no hay obstáculo
            float angleStep = config.viewAngle / config.rayCount;
            float startAngle = -config.viewAngle / 2f;

            for (int i = 0; i <= config.rayCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
                
                // Primero check de obstáculo
                if (Physics.Raycast(transform.position + Vector3.up, direction, out RaycastHit hit, config.viewRange, config.obstacleLayers | config.targetLayers))
                {
                    // Si golpeamos algo, verificamos si es un target válido (está en targetLayers y NO en obstacleLayers)
                    if (((1 << hit.collider.gameObject.layer) & config.targetLayers) != 0)
                    {
                        ProcessHit(hit.collider.gameObject, hit.distance, TargetType.Enemy); // Por ahora asumimos todo en target layer es Enemy/LootBox
                    }
                }
            }

            // 2. Hearing (OverlapSphere)
            int count = Physics.OverlapSphereNonAlloc(transform.position, config.hearingRange, hearingColliders, config.targetLayers);
            for (int i = 0; i < count; i++)
            {
                GameObject obj = hearingColliders[i].gameObject;
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                
                // Evitar añadir si ya lo vimos por raycast (podría estar doble)
                bool alreadyAdded = false;
                foreach (var target in VisibleTargets)
                {
                    if (target.transform == obj.transform)
                    {
                        alreadyAdded = true;
                        break;
                    }
                }

                if (!alreadyAdded)
                {
                    // Verificamos línea de visión por si el sonido nos alertó, aunque el sonido atraviese paredes
                    // Para simplificar, el sonido nos da posición exacta
                    ProcessHit(obj, dist, TargetType.Enemy);
                }
            }
            
            // 3. Zone Boundary (añadimos si estamos cerca del borde, simplificado)
            if (ZoneManager.Instance != null)
            {
                 var phase = ZoneManager.Instance.GetCurrentPhase();
                 if (phase != null)
                 {
                     bool inside = ZoneManager.Instance.IsInsideZone(transform.position);
                     if (!inside)
                     {
                         // Dummy target para indicar la zona
                         VisibleTargets.Add(new SensorTarget(ZoneManager.Instance.zoneVisual, 0f, TargetType.ZoneBoundary));
                     }
                 }
            }
        }

        private void ProcessHit(GameObject obj, float dist, TargetType defaultType)
        {
            if (obj == this.gameObject) return; // Ignore self
            
            // Check if it's a living entity first to ignore our own children (like ModelContainer)
            var health = obj.GetComponentInParent<HealthComponent>();
            if (health != null)
            {
                if (health.gameObject == this.gameObject) return; // It's us!

                // Filtrar companeros de equipo usando el root (health.gameObject)
                if (TeamManager.Instance != null && 
                    TeamManager.Instance.AreTeammates(gameObject, health.gameObject))
                {
                    return;
                }
            }

            TargetType type = defaultType;

            // Check if it's a loot box
            if (obj.GetComponentInParent<LootBox>() != null)
            {
                type = TargetType.LootBox;
            }
            else
            {
                if (health == null || !health.IsAlive)
                {
                    return; // Ignoramos si no tiene vida o está muerto
                }
                type = TargetType.Enemy;
            }

            // Check si ya está en la lista para no duplicar por múltiples rayos
            foreach (var t in VisibleTargets)
            {
                if (t.transform == obj.transform) return;
            }

            VisibleTargets.Add(new SensorTarget(obj.transform, dist, type));
        }
        
        private void OnDrawGizmosSelected()
        {
            if (config == null) return;
            
            // Draw Vision
            Gizmos.color = new Color(1, 1, 0, 0.2f);
            Gizmos.DrawFrustum(transform.position + Vector3.up, config.viewAngle, config.viewRange, 0.1f, 1f);
            
            // Draw Hearing
            Gizmos.color = new Color(0, 1, 1, 0.1f);
            Gizmos.DrawWireSphere(transform.position, config.hearingRange);
        }
    }
}
