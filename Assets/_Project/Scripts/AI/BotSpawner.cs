using UnityEngine;
using System.Collections.Generic;

namespace AnimalMagicRoyale.AI
{
    public class BotSpawner : MonoBehaviour
    {
        [Tooltip("The bot prefab to spawn. Must have BotController and necessary components.")]
        public GameObject botPrefab;
        
        [Tooltip("How many bots to spawn in the map.")]
        public int botCount = 5;
        
        [Tooltip("Radius around the spawner to randomly place bots.")]
        public float spawnRadius = 30f;
        
        [Tooltip("Check if we should spawn automatically on Start.")]
        public bool spawnOnStart = true;

        private List<GameObject> spawnedBots = new List<GameObject>();

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnBots(botCount);
            }
        }

        public void SpawnBots(int amount)
        {
            if (botPrefab == null)
            {
                Debug.LogError("[BotSpawner] No bot prefab assigned!");
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
                
                // Sample NavMesh to ensure it spawns on a valid pathfinding spot
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    GameObject bot = Instantiate(botPrefab, hit.position, Quaternion.identity);
                    bot.name = $"Bot_{i+1}";
                    spawnedBots.Add(bot);
                }
                else
                {
                    Debug.LogWarning($"[BotSpawner] Failed to find valid NavMesh spot near {spawnPos}");
                }
            }
        }
    }
}
