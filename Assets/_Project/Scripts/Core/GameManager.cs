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
        }

        private void OnEnable()
        {
            if (onPlayerDeath != null)
            {
                onPlayerDeath.RegisterListener(HandlePlayerDeath);
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
            }
        }

        public void UnregisterPlayer(GameObject player)
        {
            if (alivePlayers.Contains(player))
            {
                alivePlayers.Remove(player);
                NotifyAliveCount();

                if (onPlayerEliminated != null)
                {
                    onPlayerEliminated.Raise(new PlayerEliminatedPayload
                    {
                        eliminated = player,
                        remainingPlayers = alivePlayers.Count
                    });
                }
            }
        }

        private void HandlePlayerDeath(GameObject player)
        {
            UnregisterPlayer(player);
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
