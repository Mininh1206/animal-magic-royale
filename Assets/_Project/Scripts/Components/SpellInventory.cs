using UnityEngine;
using AnimalMagicRoyale.Spells;

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

            for (int i = 1; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty)
                {
                    slots[i].spellData = newSpell;
                    Debug.Log($"¡Hechizo {newSpell.name} recogido en el slot {i + 1}!");
                    return true;
                }
            }

            if (activeSlotIndex != 0)
            {
                slots[activeSlotIndex].spellData = newSpell;
                return true;
            }
            else
            {
                slots[1].spellData = newSpell;
                return true;
            }
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
                for (int i = 0; i < data.projectileCount; i++)
                {
                    Vector3 spreadDir = direction;
                    if (data.projectileCount > 1)
                    {
                        float spreadAngle = -15f + (30f * i / (data.projectileCount - 1));
                        spreadDir = Quaternion.Euler(0, spreadAngle, 0) * direction;
                    }

                    Transform effectiveFirePoint = overrideFirePoint != null ? overrideFirePoint : this.firePoint;
                    Vector3 spawnPos = effectiveFirePoint != null ? effectiveFirePoint.position : caster.transform.position + Vector3.up * 1f;
                    if (Core.ProjectilePoolManager.Instance == null)
                    {
                        Debug.LogWarning("[SpellInventory] ERROR: ProjectilePoolManager.Instance is null! Cannot spawn projectile.");
                        return false;
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
            else
            {
                Debug.LogWarning($"[SpellInventory] No se lanza proyectil físico. Speed: {data.projectileSpeed}, Prefab is null: {data.projectilePrefab == null}");
            }

            currentSlot.lastCastTime = Time.time;
            Debug.Log($"[SpellInventory] {caster.name} fired {data.spellName} from slot {activeSlotIndex}");
            return true;
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
    }
}
