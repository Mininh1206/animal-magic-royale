using System.Collections.Generic;
using UnityEngine;
using AnimalMagicRoyale.Core.Data;

namespace AnimalMagicRoyale.Core
{
    public class MatchSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject playerPrefabBase;
        public GameObject botPrefabBase;

        [Header("Spawn Points")]
        public Transform[] spawnPoints;
        
        [Header("Testing Defaults")]
        public int defaultMaxPlayers = 10;

        public void SpawnEntities()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("[MatchSpawner] No hay spawn points asignados!");
                return;
            }

            int maxPlayers = defaultMaxPlayers;
            TeamMode teamMode = TeamMode.Solo;

            if (PlayerSetupData.SelectedMap != null)
            {
                maxPlayers = PlayerSetupData.SelectedMap.maxPlayers;
                teamMode = PlayerSetupData.SelectedTeamMode;
            }

            // Extract all actual spawn points (handle nested)
            List<Transform> actualSpawnPoints = new List<Transform>();
            foreach (var sp in spawnPoints)
            {
                if (sp.childCount > 0)
                {
                    foreach (Transform child in sp)
                    {
                        actualSpawnPoints.Add(child);
                    }
                }
                else
                {
                    actualSpawnPoints.Add(sp);
                }
            }

            // Shuffle spawn points
            for (int i = 0; i < actualSpawnPoints.Count; i++)
            {
                Transform temp = actualSpawnPoints[i];
                int randomIndex = Random.Range(i, actualSpawnPoints.Count);
                actualSpawnPoints[i] = actualSpawnPoints[randomIndex];
                actualSpawnPoints[randomIndex] = temp;
            }

            // Determine teams
            int playersPerTeam = (int)teamMode;
            int totalTeams = Mathf.CeilToInt((float)maxPlayers / (float)playersPerTeam);

            List<GameObject> allSpawnedPlayers = new List<GameObject>();
            List<Vector3> usedSpawnPositions = new List<Vector3>(); // Manual tracking for distance validation
            int spawnPointIndex = 0;
            int spawnedCount = 0;

            for (int teamId = 0; teamId < totalTeams; teamId++)
            {
                if (spawnedCount >= maxPlayers) break;
                if (spawnPointIndex >= actualSpawnPoints.Count) spawnPointIndex = 0; // Wrap around if not enough points

                Transform teamSpawnBase = actualSpawnPoints[spawnPointIndex];
                spawnPointIndex++;

                for (int p = 0; p < playersPerTeam; p++)
                {
                    if (spawnedCount >= maxPlayers) break;

                    // Calculate offset for teammates and avoid overlaps
                    Vector3 spawnPos = teamSpawnBase.position;
                    float baseRadius = 1.5f;
                    int maxAttempts = 20;
                    bool foundValidPosition = false;

                    for (int attempt = 0; attempt < maxAttempts; attempt++)
                    {
                        Vector3 testPos = teamSpawnBase.position;
                        if (p > 0 || attempt > 0)
                        {
                            // Expandir el radio mucho más rápido si es de distinto equipo o si hay muchos intentos
                            float attemptRadius = baseRadius + (attempt * 2f) + (teamId * 1.5f);
                            Vector2 randomCircle = Random.insideUnitCircle * attemptRadius;
                            testPos += new Vector3(randomCircle.x, 0, randomCircle.y);
                        }

                        if (UnityEngine.AI.NavMesh.SamplePosition(testPos, out UnityEngine.AI.NavMeshHit hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            // Manual distance check against previously spawned positions
                            bool hasOverlap = false;
                            foreach (Vector3 usedPos in usedSpawnPositions)
                            {
                                if (Vector3.Distance(hit.position, usedPos) < 2.5f) // Mínimo 2.5 metros entre entidades
                                {
                                    hasOverlap = true;
                                    break;
                                }
                            }

                            if (!hasOverlap)
                            {
                                spawnPos = hit.position;
                                usedSpawnPositions.Add(spawnPos);
                                foundValidPosition = true;
                                // Debug.Log($"[MatchSpawner] Found valid spawn point for entity {spawnedCount} (Team {teamId}) at {spawnPos}. Distance check valid.");
                                break;
                            }
                        }
                    }

                    if (!foundValidPosition)
                    {
                        Debug.LogWarning($"[MatchSpawner] No se pudo encontrar un punto de aparición válido sin solapamiento para la entidad {spawnedCount}. Usando fallback.");
                    }

                    GameObject newEntity = null;

                    // The first player spawned is the local player
                    if (spawnedCount == 0 && playerPrefabBase != null)
                    {
                        newEntity = Instantiate(playerPrefabBase, spawnPos, teamSpawnBase.rotation);
                        newEntity.name = "LocalPlayer";

                        AnimalMagicRoyale.Core.Data.AnimalType currentAnimal = PlayerSetupData.SelectedAnimal;
                        if (currentAnimal == null)
                        {
                            currentAnimal = Resources.Load<AnimalMagicRoyale.Core.Data.AnimalType>("Animals/Pig/PigType");
                            // Debug.Log("[MatchSpawner] No animal selected for local player, defaulting to Pig.");
                        }

                        var skinManager = newEntity.GetComponentInChildren<SkinManager>();
                        if (skinManager != null && PlayerSetupData.SelectedSkin != null)
                        {
                            skinManager.ApplySkin(PlayerSetupData.SelectedSkin, currentAnimal);
                        }
                        else if (skinManager != null && currentAnimal != null)
                        {
                            skinManager.ApplySkin(currentAnimal.defaultSkin, currentAnimal);
                        }

                        if (currentAnimal != null)
                        {
                            var abilityHolder = newEntity.GetComponent<AnimalMagicRoyale.Components.AbilityHolder>();
                            if (abilityHolder != null)
                            {
                                abilityHolder.Initialize(currentAnimal.ability);
                            }
                        }
                    }
                    else if (botPrefabBase != null)
                    {
                        newEntity = Instantiate(botPrefabBase, spawnPos, teamSpawnBase.rotation);
                        newEntity.name = $"Bot_{spawnedCount}";
                        
                        // Assign random skin to bot
                        var skinManager = newEntity.GetComponentInChildren<SkinManager>();
                        if (skinManager != null)
                        {
                            AnimalType[] availableAnimals = Resources.LoadAll<AnimalType>("Animals");
                            if (availableAnimals != null && availableAnimals.Length > 0)
                            {
                                AnimalType randomAnimal = availableAnimals[Random.Range(0, availableAnimals.Length)];
                                SkinData randomSkin = null;

                                // Pick a random skin from the animal
                                if (randomAnimal.availableSkins != null && randomAnimal.availableSkins.Count > 0)
                                {
                                    int randIdx = Random.Range(0, randomAnimal.availableSkins.Count + (randomAnimal.defaultSkin != null ? 1 : 0));
                                    if (randIdx == randomAnimal.availableSkins.Count)
                                    {
                                        randomSkin = randomAnimal.defaultSkin;
                                    }
                                    else
                                    {
                                        randomSkin = randomAnimal.availableSkins[randIdx];
                                    }
                                }
                                else
                                {
                                    randomSkin = randomAnimal.defaultSkin;
                                }

                                if (randomSkin != null)
                                {
                                    skinManager.ApplySkin(randomSkin, randomAnimal);
                                }
                                
                                var abilityHolder = newEntity.GetComponent<AnimalMagicRoyale.Components.AbilityHolder>();
                                if (abilityHolder != null)
                                {
                                    abilityHolder.Initialize(randomAnimal.ability);
                                }
                            }
                        }
                    }

                    if (newEntity != null)
                    {
                        allSpawnedPlayers.Add(newEntity);
                        
                        if (TeamManager.Instance != null)
                        {
                            TeamManager.Instance.AssignTeam(newEntity, teamId);
                        }

                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.RegisterPlayer(newEntity);
                        }
                    }

                    spawnedCount++;
                }
            }

            // Debug.Log($"[MatchSpawner] Spawned {spawnedCount} entities across {totalTeams} teams.");
        }
    }
}
