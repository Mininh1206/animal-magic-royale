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
        private float currentRadius;
        private float targetRadius;
        private Vector3 zoneCenter;
        private bool isActive = false;
        private bool isShrinking = false;
        private float damageTimer = 0f;

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
                phase1.startRadius = 200f; phase1.endRadius = 100f; phase1.waitBeforeShrink = 10f; phase1.shrinkDuration = 20f; phase1.baseDamage = 5f;
                var phase2 = ScriptableObject.CreateInstance<ZonePhaseData>();
                phase2.startRadius = 100f; phase2.endRadius = 30f; phase2.waitBeforeShrink = 10f; phase2.shrinkDuration = 20f; phase2.baseDamage = 10f;
                var phase3 = ScriptableObject.CreateInstance<ZonePhaseData>();
                phase3.startRadius = 30f; phase3.endRadius = 5f; phase3.waitBeforeShrink = 10f; phase3.shrinkDuration = 20f; phase3.baseDamage = 20f;
                var phase4 = ScriptableObject.CreateInstance<ZonePhaseData>();
                phase4.startRadius = 5f; phase4.endRadius = 0f; phase4.waitBeforeShrink = 10f; phase4.shrinkDuration = 20f; phase4.baseDamage = 50f;
                
                phases = new ZonePhaseData[] { phase1, phase2, phase3, phase4 };
            }

            if (phases.Length > 0)
            {
                currentRadius = phases[0].startRadius;
                targetRadius = phases[0].startRadius;
            }
            zoneCenter = transform.position;
            UpdateVisuals();
        }

        public void Activate()
        {
            isActive = true;
            currentPhaseIndex = 0;
            if (phases.Length > 0)
            {
                currentRadius = phases[currentPhaseIndex].startRadius;
                targetRadius = phases[currentPhaseIndex].startRadius;
                phaseTimer = phases[currentPhaseIndex].waitBeforeShrink;
                Debug.Log($"[ZoneManager] Activated. Phase 0, start radius: {currentRadius}");
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
                    currentRadius = Mathf.Lerp(currentPhase.startRadius, currentPhase.endRadius, t);
                    UpdateVisuals();

                    if (phaseTimer <= 0)
                    {
                        AdvancePhase();
                    }
                }
            }

            ApplyZoneDamage(currentPhase);
        }

        private void StartShrinking(ZonePhaseData phase)
        {
            isShrinking = true;
            phaseTimer = phase.shrinkDuration;
            targetRadius = phase.endRadius;
            Debug.Log($"[ZoneManager] Zone shrinking to {targetRadius} over {phaseTimer}s.");

            if (onZoneShrink != null)
            {
                onZoneShrink.Raise(new ZoneShrinkPayload
                {
                    phaseIndex = currentPhaseIndex,
                    currentRadius = currentRadius,
                    targetRadius = targetRadius,
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

        private void ApplyZoneDamage(ZonePhaseData phase)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0)
            {
                damageTimer = damageTickInterval;
                
                // We'll iterate through all players registered in GameManager
                if (GameManager.Instance != null)
                {
                    // This is slightly inefficient if we could get the alive list directly, but we don't have access to the private list.
                    // We'll have to rely on objects with ZoneDamageTracker for now.
                    var trackers = FindObjectsByType<ZoneDamageTracker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                    if (trackers.Length == 0)
                    {
                        Debug.LogWarning("[ZoneManager] WARNING: No ZoneDamageTrackers found in scene!");
                    }
                    foreach (var tracker in trackers)
                    {
                        bool isInside = IsInsideZone(tracker.transform.position);
                        tracker.UpdateZoneStatus(isInside);

                        if (!isInside)
                        {
                            var health = tracker.GetComponent<HealthComponent>();
                            if (health != null && health.IsAlive)
                            {
                                float damage = phase.baseDamage * tracker.GetDamageMultiplier(phase.damageMultiplier);
                                health.TakeDamage(damage, gameObject); // Pass ZoneManager gameObject as source
                                Debug.Log($"[ZoneManager] {tracker.gameObject.name} outside zone, dealt {damage} damage.");
                            }
                        }
                    }
                }
            }
        }

        public bool IsInsideZone(Vector3 position)
        {
            Vector3 position2D = new Vector3(position.x, 0, position.z);
            Vector3 center2D = new Vector3(zoneCenter.x, 0, zoneCenter.z);
            return Vector3.Distance(position2D, center2D) <= currentRadius;
        }

        private void UpdateVisuals()
        {
            if (zoneVisual != null)
            {
                // Assuming cylinder has radius 0.5 when scale is 1
                zoneVisual.localScale = new Vector3(currentRadius * 2, 100f, currentRadius * 2);
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
