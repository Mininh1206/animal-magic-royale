using UnityEngine;

namespace AnimalMagicRoyale.Spells.Effects
{
    public abstract class SpellEffect : ScriptableObject
    {
        public abstract void Apply(GameObject caster, GameObject target);
        public virtual void Remove(GameObject target) { }
    }
}
