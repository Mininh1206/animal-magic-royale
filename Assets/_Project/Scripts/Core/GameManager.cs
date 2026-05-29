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

        public GameObject Winner { get; set; }

        private float countdownTimer;
        private bool isCountingDown = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeStateMachine();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeStateMachine()
        {
            StateMachine = new StateMachine();
            WaitingState = new WaitingState(this, StateMachine);
            PlayingState = new PlayingState(this, StateMachine);
            GameOverState = new GameOverState(this, StateMachine);
        }

        private void Start()
        {
            StateMachine.Initialize(WaitingState);
            Debug.Log($"[GameManager] Auto-starting match in {autoStartDelay} seconds...");
            Invoke(nameof(StartMatch), autoStartDelay);
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

                if (StateMachine.CurrentState == PlayingState && alivePlayers.Count <= 1)
                {
                    if (alivePlayers.Count == 1)
                    {
                        Winner = alivePlayers[0];
                    }
                    Debug.Log($"[GameManager] Match Ended! Winner: {(Winner != null ? Winner.name : "None")}");
                    StateMachine.ChangeState(GameOverState);
                }
            }
        }

        private void HandlePlayerDeath(DeathPayload payload)
        {
            Debug.Log($"[GameManager] HandlePlayerDeath received: {payload.victim?.name} killed by {payload.killer?.name ?? "environment"}");
            UnregisterPlayer(payload.victim, payload.killer);
        }

        public void StartMatch()
        {
            if (StateMachine.CurrentState == WaitingState)
            {
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
