using System.Collections.Generic;
using UnityEngine;
using AnimalMagicRoyale.Spells;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.AI
{
    public struct MemorySpellTarget
    {
        public int InstanceID;
        public Vector3 Position;
        public SpellTier Tier;
        public float Timestamp;
    }

    public struct MemoryEnemyTarget
    {
        public int InstanceID;
        public Vector3 LastKnownPosition;
        public float Timestamp;
    }

    public class TeamMemorySystem : MonoBehaviour
    {
        public static TeamMemorySystem Instance { get; private set; }

        private Dictionary<int, Dictionary<int, MemorySpellTarget>> teamSpells = new Dictionary<int, Dictionary<int, MemorySpellTarget>>();
        private Dictionary<int, Dictionary<int, MemoryEnemyTarget>> teamEnemies = new Dictionary<int, Dictionary<int, MemoryEnemyTarget>>();

        private float memoryDurationEnemy = 4f; // 4 segundos para rendirse de buscar al enemigo
        private float cleanUpInterval = 1f;
        private float lastCleanUpTime;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (Time.time - lastCleanUpTime > cleanUpInterval)
            {
                CleanUp();
                lastCleanUpTime = Time.time;
            }
        }

        public void ReportSpell(int teamId, int spellId, Vector3 pos, SpellTier tier)
        {
            if (!teamSpells.ContainsKey(teamId))
                teamSpells[teamId] = new Dictionary<int, MemorySpellTarget>();

            teamSpells[teamId][spellId] = new MemorySpellTarget
            {
                InstanceID = spellId,
                Position = pos,
                Tier = tier,
                Timestamp = Time.time
            };
            Debug.Log($"[TeamMemory] Equipo {teamId} reportó hechizo {spellId} (Tier {tier}) en {pos}");
        }

        public void RemoveSpell(int teamId, int spellId)
        {
            if (teamSpells.ContainsKey(teamId))
            {
                if (teamSpells[teamId].Remove(spellId))
                {
                    Debug.Log($"[TeamMemory] Equipo {teamId} eliminó hechizo {spellId} de la memoria (recogido o desaparecido)");
                }
            }
        }

        public Dictionary<int, MemorySpellTarget> GetSpells(int teamId)
        {
            if (teamSpells.ContainsKey(teamId))
                return teamSpells[teamId];
            return new Dictionary<int, MemorySpellTarget>();
        }

        public void ReportEnemy(int teamId, int enemyId, Vector3 lastKnownPos)
        {
            if (!teamEnemies.ContainsKey(teamId))
                teamEnemies[teamId] = new Dictionary<int, MemoryEnemyTarget>();

            teamEnemies[teamId][enemyId] = new MemoryEnemyTarget
            {
                InstanceID = enemyId,
                LastKnownPosition = lastKnownPos,
                Timestamp = Time.time
            };
            Debug.Log($"[TeamMemory] Equipo {teamId} reportó posición de enemigo {enemyId}");
        }

        public void RemoveEnemy(int teamId, int enemyId)
        {
            if (teamEnemies.ContainsKey(teamId))
            {
                if (teamEnemies[teamId].Remove(enemyId))
                {
                    Debug.Log($"[TeamMemory] Equipo {teamId} se rindió de buscar al enemigo {enemyId}");
                }
            }
        }

        public Dictionary<int, MemoryEnemyTarget> GetEnemies(int teamId)
        {
            if (teamEnemies.ContainsKey(teamId))
                return teamEnemies[teamId];
            return new Dictionary<int, MemoryEnemyTarget>();
        }

        private void CleanUp()
        {
            bool zoneActive = ZoneManager.Instance != null && ZoneManager.Instance.IsActive;

            // Limpiar enemigos caducados
            foreach (var teamDict in teamEnemies.Values)
            {
                List<int> toRemove = new List<int>();
                foreach (var kvp in teamDict)
                {
                    if (Time.time - kvp.Value.Timestamp > memoryDurationEnemy)
                    {
                        toRemove.Add(kvp.Key);
                    }
                }
                foreach (int id in toRemove)
                {
                    teamDict.Remove(id);
                }
            }

            // Limpiar hechizos fuera de la zona
            if (zoneActive)
            {
                foreach (var teamDict in teamSpells.Values)
                {
                    List<int> toRemove = new List<int>();
                    foreach (var kvp in teamDict)
                    {
                        if (!ZoneManager.Instance.IsInsideZone(kvp.Value.Position))
                        {
                            toRemove.Add(kvp.Key);
                        }
                    }
                    foreach (int id in toRemove)
                    {
                        teamDict.Remove(id);
                    }
                }
            }
        }
    }
}
