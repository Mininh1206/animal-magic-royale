using UnityEngine;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "WallhackEffect", menuName = "Animal Magic Royale/Spells/Effects/Wallhack")]
    public class WallhackEffect : SpellEffect
    {
        public float duration = 6f;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            Debug.Log($"[WallhackEffect] {target.name} activó visión a través de paredes por {duration}s.");
            
            // Wallhack es un buff para el jugador local, si este es el local:
            var input = target.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (input != null && input.enabled)
            {
                var whComp = target.AddComponent<WallhackComponent>();
                whComp.Initialize(duration);
            }
        }
    }

    public class WallhackComponent : MonoBehaviour
    {
        public void Initialize(float duration)
        {
            StartCoroutine(Routine(duration));
        }

        private System.Collections.IEnumerator Routine(float duration)
        {
            AnimalMagicRoyale.Components.UI.TeamUI.RevealEnemiesOnScreen = true;
            Debug.Log("[WallhackComponent] Wallhack Activado.");
            
            yield return new WaitForSeconds(duration);
            
            AnimalMagicRoyale.Components.UI.TeamUI.RevealEnemiesOnScreen = false;
            Debug.Log("[WallhackComponent] Wallhack Desactivado.");
            
            Destroy(this);
        }
    }
}
