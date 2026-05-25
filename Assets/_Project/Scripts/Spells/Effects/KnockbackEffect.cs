using UnityEngine;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "KnockbackEffect", menuName = "Animal Magic Royale/Spells/Effects/Knockback")]
    public class KnockbackEffect : SpellEffect
    {
        public float force;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null || caster == null) return;

            Vector3 direction = (target.transform.position - caster.transform.position).normalized;
            direction.y = 0;

            var playerController = target.GetComponent<Player.PlayerController>();
            if (playerController != null)
            {
                Debug.LogWarning("[KnockbackEffect] Knockback requires external force implementation on PlayerController.");
            }
        }
    }
}
