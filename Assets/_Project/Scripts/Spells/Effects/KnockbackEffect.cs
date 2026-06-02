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

            var kbComp = target.AddComponent<KnockbackComponent>();
            kbComp.Initialize(direction * force, 0.2f);
        }
    }

    public class KnockbackComponent : MonoBehaviour
    {
        public void Initialize(Vector3 forceVector, float duration)
        {
            StartCoroutine(KnockbackRoutine(forceVector, duration));
        }

        private System.Collections.IEnumerator KnockbackRoutine(Vector3 forceVector, float duration)
        {
            var cc = GetComponent<CharacterController>();
            var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

            if (agent != null) agent.enabled = false;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                Vector3 currentForce = Vector3.Lerp(forceVector, Vector3.zero, t);
                
                if (cc != null)
                {
                    cc.Move(currentForce * Time.deltaTime);
                }
                else
                {
                    transform.position += currentForce * Time.deltaTime;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (agent != null) agent.enabled = true;
            Destroy(this);
        }
    }
}
