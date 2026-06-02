using UnityEngine;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "InvertControlsEffect", menuName = "Animal Magic Royale/Spells/Effects/Invert Controls")]
    public class InvertControlsEffect : SpellEffect
    {
        public float duration = 3f;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            // Debug.Log($"[InvertControlsEffect] Aplicado a {target.name} por {duration}s. Controles de movimiento invertidos.");
            // TODO: Integrar con PlayerController / NavMeshAgent para invertir Input Vector
        }
    }
}
