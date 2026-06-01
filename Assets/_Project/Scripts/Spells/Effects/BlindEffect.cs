using UnityEngine;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "BlindEffect", menuName = "Animal Magic Royale/Spells/Effects/Blind")]
    public class BlindEffect : SpellEffect
    {
        public float duration = 3f;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            Debug.Log($"[BlindEffect] Aplicado a {target.name} por {duration}s.");
            
            // Si el target es el jugador local (tiene la cámara principal y el HUDManager)
            var playerInput = target.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null && playerInput.enabled)
            {
                var blindComp = target.AddComponent<BlindComponent>();
                blindComp.Initialize(duration);
            }
            else
            {
                // TODO: En el futuro, limitar visión de bots reduciendo su SensorySystem
                Debug.Log($"[BlindEffect] (Bot) Visión reducida de {target.name}");
            }
        }
    }

    public class BlindComponent : MonoBehaviour
    {
        private UnityEngine.UIElements.VisualElement smokeOverlay;

        public void Initialize(float duration)
        {
            StartCoroutine(Routine(duration));
        }

        private System.Collections.IEnumerator Routine(float duration)
        {
            var hud = FindAnyObjectByType<AnimalMagicRoyale.Components.UI.HUDManager>();
            if (hud != null)
            {
                var doc = hud.GetComponent<UnityEngine.UIElements.UIDocument>();
                if (doc != null && doc.rootVisualElement != null)
                {
                    smokeOverlay = new UnityEngine.UIElements.VisualElement();
                    smokeOverlay.style.position = UnityEngine.UIElements.Position.Absolute;
                    smokeOverlay.style.width = new UnityEngine.UIElements.StyleLength(UnityEngine.UIElements.Length.Percent(100));
                    smokeOverlay.style.height = new UnityEngine.UIElements.StyleLength(UnityEngine.UIElements.Length.Percent(100));
                    smokeOverlay.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.98f);
                    smokeOverlay.pickingMode = UnityEngine.UIElements.PickingMode.Ignore; // Para no bloquear clics
                    
                    // Insertar al fondo para que no tape otros elementos del HUD si el usuario lo desea 
                    // o tapar la vista completa si lo ponemos arriba. El requerimiento dice "por debajo del HUD"
                    doc.rootVisualElement.Insert(0, smokeOverlay);
                }
            }

            yield return new WaitForSeconds(duration);

            if (smokeOverlay != null && smokeOverlay.parent != null)
            {
                smokeOverlay.RemoveFromHierarchy();
            }

            Destroy(this);
        }
    }
}
