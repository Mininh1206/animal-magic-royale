using UnityEngine;
using AnimalMagicRoyale.Core;
using TMPro;

namespace AnimalMagicRoyale.Components.UI
{
    public class PlayerCountUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private IntEvent onAliveCountChanged;
        
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
            if (GameManager.Instance != null && countText != null)
            {
                countText.text = GameManager.Instance.AlivePlayersCount.ToString();
            }
        }

        private void HandleAliveCountChanged(int count)
        {
            if (countText != null)
            {
                countText.text = "Jugadores restantes: " + count.ToString();
            }
        }
    }
}
