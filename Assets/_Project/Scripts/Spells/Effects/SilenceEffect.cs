using UnityEngine;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Spells.Effects
{
    public enum SilenceType { SpecialAbility, SpellInventory }

    [CreateAssetMenu(fileName = "SilenceEffect", menuName = "Animal Magic Royale/Spells/Effects/Silence")]
    public class SilenceEffect : SpellEffect
    {
        public SilenceType silenceType = SilenceType.SpecialAbility;
        public float duration = 4f;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            // Debug.Log($"[SilenceEffect] Aplicado a {target.name}. Tipo: {silenceType} por {duration}s.");
            
            var silencer = target.AddComponent<SilenceComponent>();
            silencer.Initialize(duration, silenceType);
        }
    }

    public class SilenceComponent : MonoBehaviour
    {
        private AbilityHolder abilityHolder;
        private SpellInventory spellInventory;
        private SilenceType type;

        public void Initialize(float duration, SilenceType silenceType)
        {
            type = silenceType;
            abilityHolder = GetComponentInParent<AbilityHolder>();
            spellInventory = GetComponentInParent<SpellInventory>();

            if (type == SilenceType.SpecialAbility || type == (SilenceType)2) // En caso de que se expanda a "Ambos"
            {
                if (abilityHolder != null) abilityHolder.isSilenced = true;
            }
            
            if (type == SilenceType.SpellInventory || type == (SilenceType)2)
            {
                if (spellInventory != null) spellInventory.isSilenced = true;
            }

            StartCoroutine(Routine(duration));
        }

        private System.Collections.IEnumerator Routine(float duration)
        {
            yield return new WaitForSeconds(duration);
            
            if (abilityHolder != null) abilityHolder.isSilenced = false;
            if (spellInventory != null) spellInventory.isSilenced = false;
            
            Destroy(this);
        }
    }
}
