using System.Collections.Generic;
using UnityEngine;
using AnimalMagicRoyale.Spells.Effects;

namespace AnimalMagicRoyale.Spells
{
    [CreateAssetMenu(fileName = "NewSpellData", menuName = "Animal Magic Royale/Spells/Spell Data")]
    public class SpellData : ScriptableObject
    {
        [Header("Identity")]
        public string spellName;
        public SpellTier tier;
        public Sprite icon;

        [Header("Stats")]
        public float damage;
        public float cooldown;
        
        [Tooltip("0 for melee/instant spells")]
        public float projectileSpeed;
        
        public int projectileCount = 1;

        [Header("Visuals & Logic")]
        public GameObject projectilePrefab;
        public Color spellColor = Color.white; // Added for ray color
        
        [Header("Effects")]
        public List<SpellEffect> effects = new List<SpellEffect>();
    }
}
