using UnityEngine;
using AnimalMagicRoyale.Spells;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Components
{
    public class SpellInventory : MonoBehaviour
    {
        [SerializeField] private SpellData basicStickSpell;
        [SerializeField] private Transform firePoint;

        public SpellSlot[] slots = new SpellSlot[3];
        public int activeSlotIndex = 0;
        
        public Transform FirePoint => firePoint;

        public void Awake()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new SpellSlot();
            }

            if (basicStickSpell == null)
            {
                basicStickSpell = Resources.Load<SpellData>("Spells/Basic_PalodeMadera");
            }

            if (basicStickSpell != null)
            {
                slots[0].spellData = basicStickSpell;
            }
        }

        public bool TryPickupSpell(SpellData newSpell)
        {
            if (newSpell == null) return false;

            // First, try to find an empty slot (excluding the basic stick slot 0 if we want it to stay intact until explicitly overwritten)
            for (int i = 1; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty)
                {
                    slots[i].spellData = newSpell;
                    Debug.Log($"¡Hechizo {newSpell.name} recogido en el slot {i + 1}!");
                    return true;
                }
            }

            // If inventory is full, overwrite the currently active slot
            SpellData oldSpell = slots[activeSlotIndex].spellData;
            
            slots[activeSlotIndex].spellData = newSpell;
            
            // Drop the old spell
            if (oldSpell != null)
            {
                DropSpell(oldSpell);
            }
            
            return true;
        }

        private void DropSpell(SpellData spellToDrop)
        {
            if (spellToDrop == null) return;
            
            // Try to find a drop position slightly in front of the player
            Vector3 dropPosition = transform.position + transform.forward * 1.5f + Vector3.up * 0.1f;
            
            // Create the pickup object
            GameObject dropGO = new GameObject($"Dropped_{spellToDrop.spellName}");
            dropGO.transform.position = dropPosition;
            
            var pickup = dropGO.AddComponent<SpellPickup>();
            pickup.Initialize(spellToDrop);
        }

        public bool TryCast(GameObject caster, Vector3 direction, Transform overrideFirePoint = null)
        {
            Debug.Log($"[SpellInventory] TryCast called by {caster.name}. activeSlotIndex: {activeSlotIndex}");
            SpellSlot currentSlot = slots[activeSlotIndex];

            if (currentSlot.IsEmpty)
            {
                Debug.LogWarning($"[SpellInventory] Intento de disparo fallido: El slot {activeSlotIndex} está VACÍO. spellData is null: {currentSlot.spellData == null}");
                return false;
            }
            if (currentSlot.IsOnCooldown)
            {
                Debug.LogWarning($"[SpellInventory] Intento de disparo fallido: El slot {activeSlotIndex} está en ENFRIAMIENTO. Faltan {currentSlot.CooldownRemaining}s");
                return false;
            }

            SpellData data = currentSlot.spellData;
            Debug.Log($"[SpellInventory] Disparando {data.spellName} desde el slot {activeSlotIndex}. ¿Tiene prefab 3D?: {(data.projectilePrefab != null ? "SÍ" : "NO")}. Speed: {data.projectileSpeed}");

            if (data.projectileSpeed > 0 && data.projectilePrefab != null)
            {
                SpawnProjectiles(caster, direction, overrideFirePoint, data);
            }
            else
            {
                Debug.LogWarning($"[SpellInventory] No se lanza proyectil físico. Speed: {data.projectileSpeed}, Prefab is null: {data.projectilePrefab == null}");
            }

            currentSlot.lastCastTime = Time.time;
            Debug.Log($"[SpellInventory] {caster.name} fired {data.spellName} from slot {activeSlotIndex}");
            
            // Reproducir sonido de ataque
            var sfxHandler = caster.GetComponent<CharacterSFXHandler>();
            if (sfxHandler != null)
            {
                sfxHandler.PlayAttackSound();
            }

            return true;
        }

        private void SpawnProjectiles(GameObject caster, Vector3 direction, Transform overrideFirePoint, SpellData data)
        {
            Transform effectiveFirePoint = overrideFirePoint != null ? overrideFirePoint : this.firePoint;
            Vector3 spawnPos = effectiveFirePoint != null ? effectiveFirePoint.position : caster.transform.position + Vector3.up * 1f;
            
            if (Core.ProjectilePoolManager.Instance == null)
            {
                Debug.LogWarning("[SpellInventory] ERROR: ProjectilePoolManager.Instance is null! Cannot spawn projectile.");
                return;
            }

            for (int i = 0; i < data.projectileCount; i++)
            {
                Vector3 spreadDir = direction;
                if (data.projectileCount > 1)
                {
                    float spreadAngle = -15f + (30f * i / (data.projectileCount - 1));
                    spreadDir = Quaternion.Euler(0, spreadAngle, 0) * direction;
                }

                Debug.Log($"[SpellInventory] Solicitando proyectil a ProjectilePoolManager. Prefab: {data.projectilePrefab.name}");
                Projectile proj = Core.ProjectilePoolManager.Instance.GetProjectile(data.projectilePrefab);
                if (proj != null)
                {
                    proj.transform.position = spawnPos;
                    proj.Initialize(data, caster, spreadDir);
                    Debug.Log($"[SpellInventory] Proyectil inicializado y lanzado.");
                }
                else
                {
                    Debug.LogWarning($"[SpellInventory] ProjectilePoolManager devolvió null al pedir el proyectil: {data.projectilePrefab.name}");
                }
            }
        }

        public void SelectSlot(int index)
        {
            if (index >= 0 && index < slots.Length)
            {
                activeSlotIndex = index;
            }
        }

        public SpellData GetActiveSpell()
        {
            return slots[activeSlotIndex].spellData;
        }

        public void SetFirePoint(Transform newFirePoint)
        {
            this.firePoint = newFirePoint;
        }
    }
}
