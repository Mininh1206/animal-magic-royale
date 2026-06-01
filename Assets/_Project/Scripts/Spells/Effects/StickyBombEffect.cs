using UnityEngine;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "StickyBombEffect", menuName = "Animal Magic Royale/Spells/Effects/Sticky Bomb")]
    public class StickyBombEffect : SpellEffect
    {
        public float delay = 3f;
        public float radius = 4f;
        public float damage = 40f;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            Debug.Log($"[StickyBombEffect] Pegado a {target.name}. Explotará en {delay}s.");
            var bomb = target.AddComponent<StickyBombComponent>();
            bomb.Initialize(caster, delay, radius, damage);
        }
    }

    public class StickyBombComponent : MonoBehaviour
    {
        public void Initialize(GameObject caster, float delay, float radius, float damage)
        {
            StartCoroutine(Routine(caster, delay, radius, damage));
        }

        private System.Collections.IEnumerator Routine(GameObject caster, float delay, float radius, float damage)
        {
            yield return new WaitForSeconds(delay);
            
            Debug.Log($"[StickyBombComponent] ¡Explosión en {gameObject.name}!");
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            int casterTeam = -1;
            if (AnimalMagicRoyale.Core.TeamManager.Instance != null && caster != null)
                casterTeam = AnimalMagicRoyale.Core.TeamManager.Instance.GetTeam(caster);

            foreach (var hit in hits)
            {
                var hc = hit.GetComponentInParent<HealthComponent>();
                if (hc != null)
                {
                    if (casterTeam != -1 && AnimalMagicRoyale.Core.TeamManager.Instance != null && 
                        AnimalMagicRoyale.Core.TeamManager.Instance.GetTeam(hc.gameObject) == casterTeam)
                    {
                        if (hc.gameObject != caster) continue; // Si es aliado no dañar (opcional si queremos dañar caster)
                    }
                    
                    hc.TakeDamage(damage, caster);
                }
            }
            Destroy(this);
        }
    }
}
