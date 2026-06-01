using UnityEngine;
using AnimalMagicRoyale.Core;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "ChainLightningEffect", menuName = "Animal Magic Royale/Spells/Effects/Chain Lightning")]
    public class ChainLightningEffect : SpellEffect
    {
        public float damageAmount = 30f;
        public float jumpRadius = 10f;
        public int maxJumps = 3;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (caster == null || target == null) return;
            
            ApplyDamage(caster, target);
            
            Vector3 currentPos = target.transform.position;
            GameObject currentTarget = target;
            
            int casterTeam = -1;
            if (TeamManager.Instance != null) casterTeam = TeamManager.Instance.GetTeam(caster);

            for (int i = 0; i < maxJumps; i++)
            {
                Collider[] hits = Physics.OverlapSphere(currentPos, jumpRadius);
                GameObject nextTarget = null;
                float closestDist = float.MaxValue;
                
                foreach (var hit in hits)
                {
                    var hc = hit.GetComponentInParent<HealthComponent>();
                    if (hc != null && hc.gameObject != currentTarget && hc.gameObject != caster)
                    {
                        if (TeamManager.Instance != null && TeamManager.Instance.GetTeam(hc.gameObject) == casterTeam)
                            continue;
                            
                        float dist = Vector3.Distance(currentPos, hc.transform.position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            nextTarget = hc.gameObject;
                        }
                    }
                }
                
                if (nextTarget != null)
                {
                    Debug.Log($"[ChainLightningEffect] Salto a {nextTarget.name}");
                    ApplyDamage(caster, nextTarget);
                    currentTarget = nextTarget;
                    currentPos = nextTarget.transform.position;
                }
                else
                {
                    break;
                }
            }
        }

        private void ApplyDamage(GameObject caster, GameObject target)
        {
            var hc = target.GetComponentInParent<HealthComponent>();
            if (hc != null) hc.TakeDamage(damageAmount, caster);
        }
    }
}
