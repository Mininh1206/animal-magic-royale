using UnityEngine;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "SwapPositionEffect", menuName = "Animal Magic Royale/Spells/Effects/Swap Position")]
    public class SwapPositionEffect : SpellEffect
    {
        public override void Apply(GameObject caster, GameObject target)
        {
            if (caster == null || target == null) return;
            
            Debug.Log($"[SwapPositionEffect] Intercambiando posición de {caster.name} con {target.name}.");
            Vector3 temp = caster.transform.position;
            
            var targetAgent = target.GetComponent<UnityEngine.AI.NavMeshAgent>();
            var targetCC = target.GetComponent<CharacterController>();
            
            var casterAgent = caster.GetComponent<UnityEngine.AI.NavMeshAgent>();
            var casterCC = caster.GetComponent<CharacterController>();

            // Mover caster
            if (casterCC != null) casterCC.enabled = false;
            if (casterAgent != null) casterAgent.Warp(target.transform.position);
            else caster.transform.position = target.transform.position;
            if (casterCC != null) casterCC.enabled = true;

            // Mover target
            if (targetCC != null) targetCC.enabled = false;
            if (targetAgent != null) targetAgent.Warp(temp);
            else target.transform.position = temp;
            if (targetCC != null) targetCC.enabled = true;
        }
    }
}
