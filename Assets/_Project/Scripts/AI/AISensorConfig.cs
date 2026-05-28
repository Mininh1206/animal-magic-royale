using UnityEngine;

namespace AnimalMagicRoyale.AI
{
    [CreateAssetMenu(fileName = "NewSensorConfig", menuName = "Animal Magic Royale/AI/Sensor Config")]
    public class AISensorConfig : ScriptableObject
    {
        [Header("Vision")]
        [Range(0f, 360f)]
        public float viewAngle = 120f;
        public float viewRange = 25f;
        public int rayCount = 12;

        [Header("Hearing")]
        public float hearingRange = 15f;

        [Header("Performance")]
        public float sensorUpdateInterval = 0.2f;

        [Header("Layers")]
        public LayerMask obstacleLayers;
        public LayerMask targetLayers;
    }
}
