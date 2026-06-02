using UnityEngine;
using System.Collections.Generic;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "RandomDebuffEffect", menuName = "Animal Magic Royale/Spells/Effects/Random Debuff")]
    public class RandomDebuffEffect : SpellEffect
    {
        public List<SpellEffect> possibleDebuffs = new List<SpellEffect>();

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null || possibleDebuffs.Count == 0) return;
            
            int index = Random.Range(0, possibleDebuffs.Count);
            SpellEffect selected = possibleDebuffs[index];
            
            // Debug.Log($"[RandomDebuffEffect] Seleccionado debuff '{selected.name}' para aplicar a {target.name}");
            if (selected != null)
            {
                selected.Apply(caster, target);
            }
        }
    }
}
