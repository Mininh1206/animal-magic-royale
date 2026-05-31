using UnityEngine;
using AnimalMagicRoyale.Core;
using UnityEngine.UIElements;

namespace AnimalMagicRoyale.Components.UI
{
    public class PlayerCountUI : MonoBehaviour
    {
        private Label countText;
        [SerializeField] private IntEvent onAliveCountChanged;
        
        public void Initialize(VisualElement root)
        {
            countText = root.Q<Label>("lbl-player-count");
            UpdateCountText(GameManager.Instance != null ? GameManager.Instance.AlivePlayersCount : 0);
        }

        private void Awake()
        {
            if (onAliveCountChanged == null) onAliveCountChanged = Resources.Load<IntEvent>("Events/AliveCountEvent");
        }

        private void OnEnable()
        {
            if (onAliveCountChanged != null)
                onAliveCountChanged.RegisterListener(HandleAliveCountChanged);
        }
        
        private void OnDisable()
        {
            if (onAliveCountChanged != null)
                onAliveCountChanged.UnregisterListener(HandleAliveCountChanged);
        }
        
        private void Start()
        {
            if (GameManager.Instance != null)
            {
                UpdateCountText(GameManager.Instance.AlivePlayersCount);
            }
        }

        private void HandleAliveCountChanged(int count)
        {
            UpdateCountText(count);
        }

        private void UpdateCountText(int count)
        {
            if (countText != null)
            {
                countText.text = "Alive: " + count.ToString();
            }
        }
    }
}
