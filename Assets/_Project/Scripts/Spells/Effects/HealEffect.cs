using UnityEngine;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "HealEffect", menuName = "Animal Magic Royale/Spells/Effects/Heal")]
    public class HealEffect : SpellEffect
    {
        public float healAmount;
        [Tooltip("If true, it will always heal the caster instead of the target (e.g., for Life Steal).")]
        public bool healCasterInstead = false;

        public override void Apply(GameObject caster, GameObject target)
        {
            GameObject actualTarget = healCasterInstead ? caster : (target != null ? target : caster);

            if (actualTarget == null) return;

            var health = actualTarget.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.Heal(healAmount);
            }
        }
    }
}
