using UnityEngine;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "RevealOnRadarEffect", menuName = "Animal Magic Royale/Spells/Effects/Reveal On Radar")]
    public class RevealOnRadarEffect : SpellEffect
    {
        public float duration = 5f;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            // Debug.Log($"[RevealOnRadarEffect] {target.name} revela a enemigos en MinimapUI por {duration}s.");
            
            var input = target.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (input != null && input.enabled)
            {
                var radarComp = target.AddComponent<RevealOnRadarComponent>();
                radarComp.Initialize(duration);
            }
        }
    }

    public class RevealOnRadarComponent : MonoBehaviour
    {
        public void Initialize(float duration)
        {
            StartCoroutine(Routine(duration));
        }

        private System.Collections.IEnumerator Routine(float duration)
        {
            AnimalMagicRoyale.Components.UI.MinimapUI.RevealEnemiesOnMinimap = true;
            // Debug.Log("[RevealOnRadarComponent] Radar Activado.");
            
            yield return new WaitForSeconds(duration);
            
            AnimalMagicRoyale.Components.UI.MinimapUI.RevealEnemiesOnMinimap = false;
            // Debug.Log("[RevealOnRadarComponent] Radar Desactivado.");
            
            Destroy(this);
        }
    }
}
