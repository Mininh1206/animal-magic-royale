using System.Collections.Generic;
using UnityEngine;
using AnimalMagicRoyale.Core.Data;

namespace AnimalMagicRoyale.Core
{
    public class TeamManager : MonoBehaviour
    {
        public static TeamManager Instance { get; private set; }

        private Dictionary<GameObject, int> playerTeams = new Dictionary<GameObject, int>();
        private TeamMode currentMode = TeamMode.Solo;
        private int maxPlayers = 20;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetTeamConfig(TeamMode mode, int maxPlayersInMap)
        {
            currentMode = mode;
            maxPlayers = maxPlayersInMap;
            playerTeams.Clear();
            Debug.Log($"[TeamManager] Configured for Mode: {mode}, Max Players: {maxPlayers}");
        }

        public int GetMaxTeams()
        {
            return maxPlayers / (int)currentMode;
        }

        public void AssignTeam(GameObject player, int teamId)
        {
            if (playerTeams.ContainsKey(player))
            {
                playerTeams[player] = teamId;
            }
            else
            {
                playerTeams.Add(player, teamId);
            }
            Debug.Log($"[TeamManager] Assigned {player.name} to Team {teamId}");
        }

        public int GetTeam(GameObject player)
        {
            if (playerTeams.TryGetValue(player, out int teamId))
            {
                return teamId;
            }
            return -1; // Unassigned / No team
        }

        public bool AreTeammates(GameObject playerA, GameObject playerB)
        {
            // If any is null, they can't be teammates
            if (playerA == null || playerB == null) return false;

            // In Solo mode, no one is teammates with anyone else except themselves
            if (currentMode == TeamMode.Solo)
            {
                return playerA == playerB;
            }

            int teamA = GetTeam(playerA);
            int teamB = GetTeam(playerB);

            // If either doesn't have a valid team (-1), they are not teammates
            if (teamA == -1 || teamB == -1) return false;

            return teamA == teamB;
        }

        public void AutoAssignTeams(List<GameObject> players)
        {
            playerTeams.Clear();

            int currentTeam = 0;
            int currentTeamCount = 0;
            int playersPerTeam = (int)currentMode;

            foreach (var player in players)
            {
                AssignTeam(player, currentTeam);
                currentTeamCount++;

                if (currentTeamCount >= playersPerTeam)
                {
                    currentTeam++;
                    currentTeamCount = 0;
                }
            }
        }
    }
}
