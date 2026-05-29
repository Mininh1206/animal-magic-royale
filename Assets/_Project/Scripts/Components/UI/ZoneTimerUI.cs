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
        
        private float currentTimer;
        private bool isTimerRunning;
        
        private void OnEnable()
        {
            if (onZoneShrink != null)
                onZoneShrink.RegisterListener(HandleZoneShrink);
        }
        
        private void OnDisable()
        {
            if (onZoneShrink != null)
                onZoneShrink.UnregisterListener(HandleZoneShrink);
        }

        private void Update()
        {
            if (isTimerRunning && currentTimer > 0)
            {
                currentTimer -= Time.deltaTime;
                UpdateTimerText(currentTimer);
                
                if (currentTimer <= 0)
                {
                    isTimerRunning = false;
                    if (timerText != null) timerText.text = "¡Zona reduciéndose!";
                }
            }
        }

        private void HandleZoneShrink(ZoneShrinkPayload payload)
        {
            if (phaseText != null)
            {
                phaseText.text = $"Fase {payload.phaseIndex + 1}";
            }
            
            // Wait time is handled internally by ZoneManager, this event fires WHEN it starts shrinking.
            // If we want to show countdown before shrink, we need a different event or poll ZoneManager.
            // Since ZoneManager doesn't expose the phaseTimer publicly, let's just display the shrink duration for now.
            currentTimer = payload.duration;
            isTimerRunning = true;
        }
        
        private void UpdateTimerText(float time)
        {
            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(time);
                timerText.text = $"00:{seconds:00}"; // Simplified formatting
            }
        }
    }
}
