using UnityEngine;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "InvisibilityEffect", menuName = "Animal Magic Royale/Spells/Effects/Invisibility")]
    public class InvisibilityEffect : SpellEffect
    {
        public float duration = 8f;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            // Debug.Log($"[InvisibilityEffect] {target.name} se vuelve invisible por {duration}s.");
            
            var invComp = target.AddComponent<InvisibilityComponent>();
            invComp.Initialize(duration);
        }
    }

    public class InvisibilityComponent : MonoBehaviour
    {
        private AnimalMagicRoyale.Player.PlayerController playerController;
        private AnimalMagicRoyale.Core.SkinManager skinManager;

        public void Initialize(float duration)
        {
            playerController = GetComponent<AnimalMagicRoyale.Player.PlayerController>();
            skinManager = GetComponent<AnimalMagicRoyale.Core.SkinManager>();

            if (playerController != null) playerController.isInvisible = true;
            if (skinManager != null) skinManager.SetVisibility(false);

            StartCoroutine(Routine(duration));
        }

        private System.Collections.IEnumerator Routine(float duration)
        {
            yield return new WaitForSeconds(duration);
            
            if (playerController != null) playerController.isInvisible = false;
            if (skinManager != null) skinManager.SetVisibility(true);
            
            Destroy(this);
        }
    }
}
