using UnityEngine;
using System.Collections.Generic;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Core
{
    public class ZoneManager : MonoBehaviour
    {
        public static ZoneManager Instance { get; private set; }

        [Header("References")]
        public Transform zoneVisual;
        public ZoneShrinkEvent onZoneShrink;

        [Header("Settings")]
        public ZonePhaseData[] phases;
        public float damageTickInterval = 1f;

        private int currentPhaseIndex = 0;
        private float phaseTimer = 0f;
        private float currentDiameter;
        private float targetDiameter;
        private Vector3 zoneCenter;
        
        public float CurrentDiameter => currentDiameter;
        public Vector3 ZoneCenter => zoneCenter;
        private bool isActive = false;
        private bool isShrinking = false;
        private float damageTimer = 1f; // Dar un pequeño margen (1s) antes del primer tick de daño
        private CapsuleCollider zoneCollider;

        public bool IsActive => isActive;
        public bool IsShrinking => isShrinking;
        public float PhaseTimer => phaseTimer;
        public int CurrentPhaseIndex => currentPhaseIndex;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (onZoneShrink == null) onZoneShrink = Resources.Load<ZoneShrinkEvent>("Events/ZoneShrinkEvent");
                
                // Asegurarse de que el visual existe antes de asignarle el collider
                if (zoneVisual != null)
                {
                    // Destruir colliders que pudieran venir por defecto en el modelo (ej. cilindro de Unity) para que no sean obstáculos físicos
                    foreach (var col in zoneVisual.gameObject.GetComponents<Collider>())
                    {
                        Destroy(col);
                    }

                    zoneCollider = zoneVisual.gameObject.AddComponent<CapsuleCollider>();
                    zoneCollider.isTrigger = true;
                    zoneCollider.height = 20f; // Multiplicado por localScale.y (100) = 2000 de altura
                    zoneCollider.radius = 0.5f; // Multiplicado por localScale.x (currentDiameter) = radio real
                    zoneCollider.direction = 1; // Y-Axis
                    
                    var proxy = zoneVisual.gameObject.AddComponent<ZoneDamageColliderProxy>();
                    proxy.Initialize(this);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (AnimalMagicRoyale.Core.Data.PlayerSetupData.SelectedMap != null && AnimalMagicRoyale.Core.Data.PlayerSetupData.SelectedMap.zonePhases != null && AnimalMagicRoyale.Core.Data.PlayerSetupData.SelectedMap.zonePhases.Count > 0)
            {
                phases = AnimalMagicRoyale.Core.Data.PlayerSetupData.SelectedMap.zonePhases.ToArray();
            }
            else if (phases == null || phases.Length == 0)
            {
                // Fallback para pruebas rápidas en escena
                var phase1 = ScriptableObject.CreateInstance<ZonePhaseData>();
                phase1.startDiameter = 400f; phase1.endDiameter = 200f; phase1.waitBeforeShrink = 10f; phase1.shrinkDuration = 20f; phase1.baseDamage = 5f;
                var phase2 = ScriptableObject.CreateInstance<ZonePhaseData>();
                phase2.startDiameter = 200f; phase2.endDiameter = 60f; phase2.waitBeforeShrink = 10f; phase2.shrinkDuration = 20f; phase2.baseDamage = 10f;
                var phase3 = ScriptableObject.CreateInstance<ZonePhaseData>();
                phase3.startDiameter = 60f; phase3.endDiameter = 10f; phase3.waitBeforeShrink = 10f; phase3.shrinkDuration = 20f; phase3.baseDamage = 20f;
                var phase4 = ScriptableObject.CreateInstance<ZonePhaseData>();
                phase4.startDiameter = 10f; phase4.endDiameter = 0f; phase4.waitBeforeShrink = 10f; phase4.shrinkDuration = 20f; phase4.baseDamage = 50f;
                
                phases = new ZonePhaseData[] { phase1, phase2, phase3, phase4 };
            }

            if (phases.Length > 0)
            {
                currentDiameter = phases[0].startDiameter;
                targetDiameter = phases[0].startDiameter;
            }

            // Usar la posición del visual de la zona como centro (por si el manager está en otra parte)
            zoneCenter = zoneVisual != null ? zoneVisual.position : transform.position;
            UpdateVisuals();
        }

        public void Activate()
        {
            isActive = true;
            currentPhaseIndex = 0;
            if (phases.Length > 0)
            {
                currentDiameter = phases[currentPhaseIndex].startDiameter;
                targetDiameter = phases[currentPhaseIndex].startDiameter;
                phaseTimer = phases[currentPhaseIndex].waitBeforeShrink;
                Debug.Log($"[ZoneManager] Activated. Phase 0, start diameter: {currentDiameter}");
            }
            else
            {
                Debug.LogWarning("[ZoneManager] Activated but no phases are defined!");
            }
            isShrinking = false;
        }

        public void Deactivate()
        {
            isActive = false;
        }

        private void Update()
        {
            if (!isActive || phases.Length == 0) return;

            int activePhaseIndex = Mathf.Min(currentPhaseIndex, phases.Length - 1);
            ZonePhaseData currentPhase = phases[activePhaseIndex];

            if (currentPhaseIndex < phases.Length)
            {
                if (!isShrinking)
                {
                    phaseTimer -= Time.deltaTime;
                    if (phaseTimer <= 0)
                    {
                        StartShrinking(currentPhase);
                    }
                }
                else
                {
                    phaseTimer -= Time.deltaTime;
                    float t = 1f - (phaseTimer / currentPhase.shrinkDuration);
                    currentDiameter = Mathf.Lerp(currentPhase.startDiameter, currentPhase.endDiameter, t);
                    UpdateVisuals();

                    if (phaseTimer <= 0)
                    {
                        AdvancePhase();
                    }
                }
            }

            UpdateTrackers();
            ApplyZoneDamage(currentPhase);
        }

        private void StartShrinking(ZonePhaseData phase)
        {
            isShrinking = true;
            phaseTimer = phase.shrinkDuration;
            targetDiameter = phase.endDiameter;
            Debug.Log($"[ZoneManager] Zone shrinking to {targetDiameter} over {phaseTimer}s.");

            if (onZoneShrink != null)
            {
                onZoneShrink.Raise(new ZoneShrinkPayload
                {
                    phaseIndex = currentPhaseIndex,
                    currentRadius = currentDiameter / 2f,
                    targetRadius = targetDiameter / 2f,
                    duration = phase.shrinkDuration
                });
            }
        }

        private void AdvancePhase()
        {
            currentPhaseIndex++;
            isShrinking = false;

            if (currentPhaseIndex < phases.Length)
            {
                phaseTimer = phases[currentPhaseIndex].waitBeforeShrink;
                Debug.Log($"[ZoneManager] Advanced to Phase {currentPhaseIndex}. Waiting {phaseTimer}s before shrink.");
            }
            else
            {
                Debug.Log("[ZoneManager] Final phase reached. Zone will no longer shrink.");
            }
        }

        private void UpdateTrackers()
        {
            if (GameManager.Instance != null)
            {
                var trackers = FindObjectsByType<ZoneDamageTracker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var tracker in trackers)
                {
                    bool inside = IsInsideZone(tracker.transform.position);
                    tracker.UpdateZoneStatus(inside);
                }
            }
        }

        private void ApplyZoneDamage(ZonePhaseData phase)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0)
            {
                damageTimer = damageTickInterval;
                
                if (GameManager.Instance != null)
                {
                    var trackers = FindObjectsByType<ZoneDamageTracker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                    foreach (var tracker in trackers)
                    {
                        if (tracker.isOutside)
                        {
                            var health = tracker.GetComponent<HealthComponent>();
                            if (health != null && health.IsAlive)
                            {
                                float damage = phase.baseDamage * tracker.GetDamageMultiplier(phase.damageMultiplier);
                                health.TakeDamage(damage, gameObject); // Pass ZoneManager gameObject as source
                            }
                        }
                    }
                }
            }
        }

        public void HandleTriggerExit(Collider other)
        {
            if (other.TryGetComponent<ZoneDamageTracker>(out var tracker))
            {
                tracker.UpdateZoneStatus(false); // No está dentro
            }
        }

        public void HandleTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<ZoneDamageTracker>(out var tracker))
            {
                tracker.UpdateZoneStatus(true); // Está dentro
            }
        }

        public bool IsInsideZone(Vector3 position)
        {
            Vector3 position2D = new Vector3(position.x, 0, position.z);
            Vector3 center2D = new Vector3(zoneCenter.x, 0, zoneCenter.z);
            return Vector3.Distance(position2D, center2D) <= (currentDiameter / 2f);
        }

        private void UpdateVisuals()
        {
            if (zoneVisual != null)
            {
                // El collider hereda esta escala. Al tener radio 0.5, el radio físico se ajusta a currentDiameter/2.
                zoneVisual.localScale = new Vector3(currentDiameter, 100f, currentDiameter);
            }
        }

        public ZonePhaseData GetCurrentPhase()
        {
            if (currentPhaseIndex < phases.Length)
                return phases[currentPhaseIndex];
            return null;
        }
    }
}
