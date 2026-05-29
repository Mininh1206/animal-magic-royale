using UnityEngine;
using AnimalMagicRoyale.Core;
using TMPro;

namespace AnimalMagicRoyale.Components.UI
{
    public class ZoneTimerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI phaseText;
        [SerializeField] private ZoneShrinkEvent onZoneShrink;
        
        private void Update()
        {
            if (ZoneManager.Instance != null && ZoneManager.Instance.IsActive)
            {
                if (phaseText != null)
                {
                    phaseText.text = $"Fase {ZoneManager.Instance.CurrentPhaseIndex + 1}";
                }

                if (ZoneManager.Instance.IsShrinking)
                {
                    if (timerText != null) timerText.text = "¡La zona se está cerrando!";
                }
                else
                {
                    float time = ZoneManager.Instance.PhaseTimer;
                    if (timerText != null)
                    {
                        int seconds = Mathf.CeilToInt(time);
                        timerText.text = $"00:{seconds:00}";
                    }
                }
            }
            else
            {
                if (timerText != null) timerText.text = "Esperando...";
                if (phaseText != null) phaseText.text = "";
            }
        }
    }
}
