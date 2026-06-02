using UnityEngine;
using System.Collections;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "FireRateModifierEffect", menuName = "Animal Magic Royale/Spells/Effects/Fire Rate Modifier")]
    public class FireRateModifierEffect : SpellEffect
    {
        public float multiplier = 1.25f; // >1 is buff (e.g. 1.25 = 25% faster cooldown recovery), <1 is debuff
        public float duration = 10f;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            var inventory = target.GetComponentInParent<SpellInventory>();
            if (inventory != null)
            {
                // Debug.Log($"[FireRateModifierEffect] Aplicado a {target.name}. Modificador {multiplier}x por {duration}s.");
                var modifier = target.AddComponent<FireRateModifierComponent>();
                modifier.Initialize(multiplier, duration);
            }
        }
    }

    public class FireRateModifierComponent : MonoBehaviour
    {
        private float originalMultiplier = 1f;
        private float multiplier;
        // Nota: En SpellInventory habría que leer un global cooldown multiplier.
        // Como no existe, simularemos reduciendo el CooldownRemaining de las slots activamente 
        // o logueándolo como TODO para la arquitectura actual de SpellSlot.
        
        public void Initialize(float mul, float dur)
        {
            multiplier = mul;
            StartCoroutine(Routine(dur));
        }

        private IEnumerator Routine(float duration)
        {
            // Debug.Log($"[FireRateModifierComponent] Iniciado con multiplicador {multiplier}");
            yield return new WaitForSeconds(duration);
            // Debug.Log($"[FireRateModifierComponent] Finalizado.");
            Destroy(this);
        }
    }
}
