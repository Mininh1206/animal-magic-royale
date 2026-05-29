using UnityEngine;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "DamageEffect", menuName = "Animal Magic Royale/Spells/Effects/Damage")]
    public class DamageEffect : SpellEffect
    {
        public float damageAmount;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            
            var health = target.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.TakeDamage(damageAmount, caster);
            }
        }
    }
}
