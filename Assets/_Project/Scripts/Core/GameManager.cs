using System.Collections.Generic;
using UnityEngine;

namespace AnimalMagicRoyale.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Events")]
        public GameStateEvent onGameStateChanged;
        public IntEvent onAliveCountChanged;
        public DeathEvent onPlayerDeath;
        public MatchStartEvent onMatchStart;
        public PlayerEliminatedEvent onPlayerEliminated;

        [Header("Settings")]
        public float countdownDuration = 3f;
        public float autoStartDelay = 2f;

        public StateMachine StateMachine { get; private set; }
        public WaitingState WaitingState { get; private set; }
        public PlayingState PlayingState { get; private set; }
        public GameOverState GameOverState { get; private set; }

        private List<GameObject> alivePlayers = new List<GameObject>();
        public int AlivePlayersCount => alivePlayers.Count;
        public int TotalPlayers { get; private set; }
        public IReadOnlyList<GameObject> AlivePlayers => alivePlayers;

        public GameObject Winner { get; set; }

        private float countdownTimer;
        private bool isCountingDown = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (transform.parent != null)
                {
                    transform.SetParent(null);
                }

                if (TeamManager.Instance == null)
                {
                    GameObject tmObj = new GameObject("TeamManager");
                    tmObj.AddComponent<TeamManager>();
                }

                InitializeStateMachine();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeStateMachine()
        {
            // Fallback for events
            if (onGameStateChanged == null) onGameStateChanged = Resources.Load<GameStateEvent>("Events/GameStateEvent");
            if (onAliveCountChanged == null) onAliveCountChanged = Resources.Load<IntEvent>("Events/AliveCountEvent");
            if (onPlayerDeath == null) onPlayerDeath = Resources.Load<DeathEvent>("Events/DeathEvent");
            if (onMatchStart == null) onMatchStart = Resources.Load<MatchStartEvent>("Events/MatchStartEvent");
            if (onPlayerEliminated == null) onPlayerEliminated = Resources.Load<PlayerEliminatedEvent>("Events/PlayerEliminatedEvent");

            StateMachine = new StateMachine();
            WaitingState = new WaitingState(this, StateMachine);
            PlayingState = new PlayingState(this, StateMachine);
            GameOverState = new GameOverState(this, StateMachine);
        }

        private void Start()
        {
            StateMachine.Initialize(WaitingState);
            
            if (AnimalMagicRoyale.Core.Data.PlayerSetupData.SelectedMap != null)
            {
                Debug.Log("[GameManager] Map selected from lobby. Initializing match...");
                // Usamos Invoke para darle un pequeñísimo margen al resto de Awake/Starts de la escena
                Invoke(nameof(InitializeFromLobby), 0.1f);
            }
            else
            {
                Debug.Log($"[GameManager] Auto-starting match in {autoStartDelay} seconds...");
                Invoke(nameof(StartMatch), autoStartDelay);
            }
        }

        public void InitializeFromLobby()
        {
            if (TeamManager.Instance != null && AnimalMagicRoyale.Core.Data.PlayerSetupData.SelectedMap != null)
            {
                TeamManager.Instance.SetTeamConfig(
                    AnimalMagicRoyale.Core.Data.PlayerSetupData.SelectedTeamMode, 
                    AnimalMagicRoyale.Core.Data.PlayerSetupData.SelectedMap.maxPlayers
                );
            }
            StartMatch();
        }

        private void OnEnable()
        {
            if (onPlayerDeath != null)
            {
                onPlayerDeath.RegisterListener(HandlePlayerDeath);
                Debug.Log($"[GameManager] Subscribed to DeathEvent (asset: {onPlayerDeath.name})");
            }
            else
            {
                Debug.LogWarning("[GameManager] onPlayerDeath is NULL! Cannot listen for deaths. KillFeed chain broken.");
            }
        }

        private void OnDisable()
        {
            if (onPlayerDeath != null)
            {
                onPlayerDeath.UnregisterListener(HandlePlayerDeath);
            }
        }

        private void Update()
        {
            if (isCountingDown)
            {
                countdownTimer -= Time.deltaTime;
                if (countdownTimer <= 0)
                {
                    isCountingDown = false;
                    StateMachine.ChangeState(PlayingState);
                }
            }

            StateMachine.Update();
        }

        public void RegisterPlayer(GameObject player)
        {
            if (!alivePlayers.Contains(player))
            {
                alivePlayers.Add(player);
                TotalPlayers = alivePlayers.Count;
                NotifyAliveCount();
                Debug.Log($"[GameManager] Registered: {player.name}. Alive: {alivePlayers.Count}");
            }
        }

        public void UnregisterPlayer(GameObject player, GameObject killer = null)
        {
            if (alivePlayers.Contains(player))
            {
                alivePlayers.Remove(player);
                NotifyAliveCount();
                Debug.Log($"[GameManager] Player eliminated: {player.name} by {killer?.name ?? "environment"}. Remaining: {alivePlayers.Count}");

                if (onPlayerEliminated != null)
                {
                    onPlayerEliminated.Raise(new PlayerEliminatedPayload
                    {
                        eliminated = player,
                        killer = killer,
                        remainingPlayers = alivePlayers.Count
                    });
                }

                if (StateMachine.CurrentState == PlayingState)
                {
                    CheckWinCondition();
                }
            }
        }

        private void CheckWinCondition()
        {
            if (alivePlayers.Count == 0)
            {
                Debug.Log($"[GameManager] Match Ended! Draw!");
                StateMachine.ChangeState(GameOverState);
                return;
            }

            if (TeamManager.Instance != null)
            {
                int firstTeamId = TeamManager.Instance.GetTeam(alivePlayers[0]);
                bool allSameTeam = true;
                
                foreach (var p in alivePlayers)
                {
                    if (TeamManager.Instance.GetTeam(p) != firstTeamId)
                    {
                        allSameTeam = false;
                        break;
                    }
                }

                if (allSameTeam)
                {
                    Winner = alivePlayers[0];
                    Debug.Log($"[GameManager] Match Ended! Winning Team: {firstTeamId}");
                    StateMachine.ChangeState(GameOverState);
                }
            }
            else
            {
                if (alivePlayers.Count == 1)
                {
                    Winner = alivePlayers[0];
                    Debug.Log($"[GameManager] Match Ended! Winner: {Winner.name}");
                    StateMachine.ChangeState(GameOverState);
                }
            }
        }

        private void HandlePlayerDeath(DeathPayload payload)
        {
            Debug.Log($"[GameManager] HandlePlayerDeath received: {payload.victim?.name} killed by {payload.killer?.name ?? "environment"}");
            
            if (payload.victim != null)
            {
                // Lógica de espectador si muere el jugador local
                var playerController = payload.victim.GetComponent<AnimalMagicRoyale.Player.PlayerController>();
                if (playerController != null)
                {
                    HandleLocalPlayerDeathSpectator(payload.victim);
                }
            }
            
            UnregisterPlayer(payload.victim, payload.killer);
        }

        private void HandleLocalPlayerDeathSpectator(GameObject localPlayer)
        {
            GameObject teammate = GetAliveTeammate(localPlayer);
            if (teammate != null)
            {
                var vcam = localPlayer.GetComponentInChildren<Unity.Cinemachine.CinemachineCamera>(true);
                if (vcam != null && vcam.transform.parent != null)
                {
                    // Desvincular para que no se desactive cuando localPlayer.SetActive(false) ocurra
                    vcam.transform.parent.SetParent(null);
                    
                    Debug.Log($"[Spectator] Local player died. Spectating teammate: {teammate.name}");
                    vcam.Follow = teammate.transform;
                    vcam.LookAt = teammate.transform;
                }
            }
            else
            {
                Debug.Log("[Spectator] No alive teammates found. Proceeding to Game Over.");
                StateMachine.ChangeState(GameOverState);
            }
        }

        private GameObject GetAliveTeammate(GameObject player)
        {
            if (TeamManager.Instance == null) return null;

            int myTeam = TeamManager.Instance.GetTeam(player);
            if (myTeam == -1) return null;

            foreach (var alivePlayer in alivePlayers)
            {
                if (alivePlayer != player && TeamManager.Instance.GetTeam(alivePlayer) == myTeam)
                {
                    return alivePlayer;
                }
            }
            return null;
        }

        public void StartMatch()
        {
            if (StateMachine.CurrentState == WaitingState)
            {
                var spawner = FindFirstObjectByType<MatchSpawner>();
                if (spawner != null)
                {
                    spawner.SpawnEntities();
                }

                isCountingDown = true;
                countdownTimer = countdownDuration;
                Debug.Log($"[GameManager] Match starting in {countdownDuration} seconds...");
            }
        }

        private void NotifyAliveCount()
        {
            if (onAliveCountChanged != null)
            {
                onAliveCountChanged.Raise(alivePlayers.Count);
            }
        }

        public GameObject GetLastAlivePlayer()
        {
            if (alivePlayers.Count == 1)
            {
                return alivePlayers[0];
            }
            return null;
        }
    }
}
