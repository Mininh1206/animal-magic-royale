using UnityEngine;
using AnimalMagicRoyale.Components;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "DisarmEffect", menuName = "Animal Magic Royale/Spells/Effects/Disarm")]
    public class DisarmEffect : SpellEffect
    {
        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            Debug.Log($"[DisarmEffect] Aplicado a {target.name}. Soltando hechizo actual.");
            var inventory = target.GetComponentInParent<SpellInventory>();
            if (inventory != null)
            {
                inventory.DropActiveSpell();
            }
        }
    }
}
