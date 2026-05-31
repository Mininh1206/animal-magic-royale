using UnityEngine;
using AnimalMagicRoyale.Core;
using UnityEngine.UIElements;

namespace AnimalMagicRoyale.Components.UI
{
    public class ZoneTimerUI : MonoBehaviour
    {
        private Label timerText;
        private Label phaseText;
        [SerializeField] private ZoneShrinkEvent onZoneShrink;
        
        public void Initialize(VisualElement root)
        {
            timerText = root.Q<Label>("lbl-zone-time");
            phaseText = root.Q<Label>("lbl-zone-status");
        }

        private void Awake()
        {
            if (onZoneShrink == null) onZoneShrink = Resources.Load<ZoneShrinkEvent>("Events/ZoneShrinkEvent");
        }

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
