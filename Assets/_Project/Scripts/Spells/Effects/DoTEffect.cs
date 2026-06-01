using UnityEngine;
using System.Collections;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "DoTEffect", menuName = "Animal Magic Royale/Spells/Effects/Damage Over Time")]
    public class DoTEffect : SpellEffect
    {
        public float dps = 2f;
        public float duration = 5f;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            var hc = target.GetComponentInParent<HealthComponent>();
            if (hc != null)
            {
                Debug.Log($"[DoTEffect] Aplicado a {target.name}. {dps} daño/s por {duration}s.");
                var dot = target.AddComponent<DoTComponent>();
                dot.Initialize(hc, caster, dps, duration);
            }
        }
    }

    public class DoTComponent : MonoBehaviour
    {
        private HealthComponent health;
        private GameObject caster;
        private float dps;

        public void Initialize(HealthComponent hc, GameObject c, float damagePerSecond, float dur)
        {
            health = hc;
            caster = c;
            dps = damagePerSecond;
            StartCoroutine(Routine(dur));
        }

        private IEnumerator Routine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                yield return new WaitForSeconds(1f);
                if (health != null) health.TakeDamage(dps, caster);
                elapsed += 1f;
            }
            Destroy(this);
        }
    }
}
