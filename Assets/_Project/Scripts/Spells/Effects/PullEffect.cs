using UnityEngine;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "PullEffect", menuName = "Animal Magic Royale/Spells/Effects/Pull")]
    public class PullEffect : SpellEffect
    {
        public override void Apply(GameObject caster, GameObject target)
        {
            if (caster == null || target == null) return;
            Debug.Log($"[PullEffect] Acercando {target.name} a {caster.name}");
            
            var puller = target.AddComponent<PullComponent>();
            puller.Initialize(caster.transform, 0.5f);
        }
    }

    public class PullComponent : MonoBehaviour
    {
        public void Initialize(Transform targetDest, float duration)
        {
            StartCoroutine(PullRoutine(targetDest, duration));
        }

        private System.Collections.IEnumerator PullRoutine(Transform targetDest, float duration)
        {
            var cc = GetComponent<CharacterController>();
            var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

            if (cc != null) cc.enabled = false;
            if (agent != null) agent.enabled = false;

            Vector3 startPos = transform.position;
            float elapsed = 0f;

            Debug.Log($"[PullComponent] Iniciando pull sobre {gameObject.name}");

            while (elapsed < duration)
            {
                if (targetDest == null) break;
                
                // Pull close but not exactly inside the caster (leave 1m distance)
                Vector3 destPos = targetDest.position;
                Vector3 dir = (transform.position - destPos).normalized;
                if (dir == Vector3.zero) dir = Vector3.forward;
                Vector3 finalDest = destPos + dir * 1f;

                transform.position = Vector3.Lerp(startPos, finalDest, elapsed / duration);
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            Debug.Log($"[PullComponent] Pull finalizado sobre {gameObject.name}");

            if (cc != null) cc.enabled = true;
            if (agent != null) agent.enabled = true;

            Destroy(this);
        }
    }
}
