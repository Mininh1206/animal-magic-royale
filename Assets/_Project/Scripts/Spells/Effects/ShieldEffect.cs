using UnityEngine;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "ShieldEffect", menuName = "Animal Magic Royale/Spells/Effects/Shield")]
    public class ShieldEffect : SpellEffect
    {
        public float shieldAmount = 50f;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            var hc = target.GetComponentInParent<HealthComponent>();
            if (hc != null)
            {
                // Debug.Log($"[ShieldEffect] Aplicado a {target.name}. +{shieldAmount} escudo temporal.");
                var shield = target.AddComponent<AnimalMagicRoyale.Components.Abilities.ShieldComponent>();
                shield.Initialize(shieldAmount, 10f); // 10s default duration if none provided
            }
        }
    }
}
