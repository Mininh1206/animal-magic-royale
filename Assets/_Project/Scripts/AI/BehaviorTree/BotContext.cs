using UnityEngine;

namespace AnimalMagicRoyale.AI
{
    public class BotContext
    {
        public BotController Bot;
        public AISensorSystem Sensor;
        public FuzzyOutput FuzzyResult;
        
        // Blackboard items for actions
        public Transform CurrentTarget;
        public Vector3 FleeDestination;
        public SensorTarget? NearestEnemy;
        public SensorTarget? NearestLootBox;
    }
}
