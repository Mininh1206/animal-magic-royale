using UnityEngine;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "HardStunEffect", menuName = "Animal Magic Royale/Spells/Effects/Hard Stun")]
    public class HardStunEffect : SpellEffect
    {
        public float duration = 1.5f;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            
            // TODO: Integrar con la StateMachine para forzar estado HardStunned
            // Provisionalmente logueamos
            Debug.Log($"[HardStunEffect] Aplicado a {target.name} por {duration}s. Bloqueo total de input.");
        }
    }
}
