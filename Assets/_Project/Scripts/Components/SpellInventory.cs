using UnityEngine;
using AnimalMagicRoyale.Spells;

namespace AnimalMagicRoyale.Components
{
    public class SpellInventory : MonoBehaviour
    {
        [SerializeField] private SpellData basicStickSpell;

        public SpellSlot[] slots = new SpellSlot[3];
        public int activeSlotIndex = 0;

        public void Awake()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new SpellSlot();
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

        public bool TryCast(GameObject caster, Vector3 direction, Transform firePoint = null)
        {
            SpellSlot currentSlot = slots[activeSlotIndex];

            if (currentSlot.IsEmpty)
            {
                Debug.LogWarning($"[SpellInventory] Intento de disparo fallido: El slot {activeSlotIndex} está VACÍO.");
                return false;
            }
            if (currentSlot.IsOnCooldown)
            {
                Debug.LogWarning($"[SpellInventory] Intento de disparo fallido: El slot {activeSlotIndex} está en ENFRIAMIENTO.");
                return false;
            }

            SpellData data = currentSlot.spellData;
            Debug.Log($"[SpellInventory] Disparando {data.spellName} desde el slot {activeSlotIndex}. ¿Tiene prefab 3D?: {(data.projectilePrefab != null ? "SÍ" : "NO")}");

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

                    Vector3 spawnPos = firePoint != null ? firePoint.position : caster.transform.position + Vector3.up * 1f;
                    Projectile proj = Core.ProjectilePoolManager.Instance.GetProjectile(data.projectilePrefab);
                    if (proj != null)
                    {
                        proj.transform.position = spawnPos;
                        proj.Initialize(data, caster, spreadDir);
                    }
                }
            }

            currentSlot.lastCastTime = Time.time;
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
