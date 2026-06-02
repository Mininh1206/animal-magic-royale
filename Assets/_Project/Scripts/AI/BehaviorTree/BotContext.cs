using UnityEngine;

namespace AnimalMagicRoyale.AI
{
    public class BotContext
    {
        public BotController Bot;
        public AISensorSystem Sensor;
        public FuzzyOutput FuzzyResult;
        
        // Context Info
        public int TeamId;
        public Transform CurrentRevengeTarget;
        public bool IsTakingDamage;

        // Blackboard items for actions
        public Transform NearestEnemy;
        
        // Loot specific properties
        public Transform BestPhysicalLoot;
        public MemorySpellTarget? BestMemoryLoot;
        
        // Investigation specific properties
        public MemoryEnemyTarget? InvestigationTarget;

        public float NextAttackTime;
    }
}
