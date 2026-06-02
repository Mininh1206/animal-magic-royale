using UnityEngine;
using AnimalMagicRoyale.Core;
using UnityEngine.UIElements;

namespace AnimalMagicRoyale.Components.UI
{
    public class PlayerCountUI : MonoBehaviour
    {
        private Label countText;
        private Label killText;
        
        [SerializeField] private IntEvent onAliveCountChanged;
        [SerializeField] private PlayerEliminatedEvent onPlayerEliminated;
        
        private int killCount = 0;
        
        public void Initialize(VisualElement root)
        {
            countText = root.Q<Label>("lbl-player-count");
            killText = root.Q<Label>("lbl-kill-count");
            
            UpdateCountText(GameManager.Instance != null ? GameManager.Instance.AlivePlayersCount : 0);
            UpdateKillText();
        }

        private void Awake()
        {
            if (onAliveCountChanged == null) onAliveCountChanged = Resources.Load<IntEvent>("Events/AliveCountEvent");
            if (onPlayerEliminated == null) onPlayerEliminated = Resources.Load<PlayerEliminatedEvent>("Events/PlayerEliminatedEvent");
        }

        private void OnEnable()
        {
            if (onAliveCountChanged != null)
                onAliveCountChanged.RegisterListener(HandleAliveCountChanged);
                
            if (onPlayerEliminated != null)
                onPlayerEliminated.RegisterListener(HandlePlayerEliminated);
        }
        
        private void OnDisable()
        {
            if (onAliveCountChanged != null)
                onAliveCountChanged.UnregisterListener(HandleAliveCountChanged);
                
            if (onPlayerEliminated != null)
                onPlayerEliminated.UnregisterListener(HandlePlayerEliminated);
        }
        
        private void Start()
        {
            if (GameManager.Instance != null)
            {
                UpdateCountText(GameManager.Instance.AlivePlayersCount);
            }
            killCount = 0;
            UpdateKillText();
        }

        private void HandleAliveCountChanged(int count)
        {
            UpdateCountText(count);
        }
        
        private void HandlePlayerEliminated(PlayerEliminatedPayload payload)
        {
            // Check if the local player is the killer
            if (payload.killer != null && payload.killer != payload.eliminated)
            {
                var playerController = payload.killer.GetComponent<AnimalMagicRoyale.Player.PlayerController>();
                if (playerController != null)
                {
                    // Yes, it was the local player
                    killCount++;
                    UpdateKillText();
                }
            }
        }

        private void UpdateCountText(int count)
        {
            if (countText != null)
            {
                countText.text = count.ToString() + " ALIVE";
            }
        }
        
        private void UpdateKillText()
        {
            if (killText != null)
            {
                killText.text = killCount.ToString() + (killCount == 1 ? " KILL" : " KILLS");
            }
        }
    }
}
