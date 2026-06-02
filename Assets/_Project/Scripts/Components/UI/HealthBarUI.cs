using UnityEngine;
using UnityEngine.UI;
using AnimalMagicRoyale.Core;
using UnityEngine.UIElements;

namespace AnimalMagicRoyale.Components.UI
{
    /// <summary>
    /// Conecta la barra de vida (Image tipo Filled) con el HealthComponent del jugador
    /// a través del evento HealthChangedEvent del EventBus.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        private GameObject trackedPlayer;
        private UnityEngine.UIElements.VisualElement healthFill;
        private UnityEngine.UIElements.Label healthLabel;

        [Header("Events")]
        [SerializeField] private HealthChangedEvent onHealthChanged;

        private void Awake()
        {
            if (onHealthChanged == null) onHealthChanged = Resources.Load<HealthChangedEvent>("Events/HealthChangedEvent");
        }

        private void OnEnable()
        {
            if (onHealthChanged != null)
                onHealthChanged.RegisterListener(HandleHealthChanged);
        }

        private void OnDisable()
        {
            if (onHealthChanged != null)
                onHealthChanged.UnregisterListener(HandleHealthChanged);
        }

        public void Initialize(VisualElement root)
        {
            healthFill = root.Q<VisualElement>("health-bar-fill");
            healthLabel = root.Q<Label>("lbl-health-value");
            
            if (healthFill != null)
            {
                healthFill.style.width = Length.Percent(100f);
            }
            if (healthLabel != null)
            {
                healthLabel.text = "100";
            }
        }

        private void HandleHealthChanged(HealthChangedPayload payload)
        {
            if (trackedPlayer != null && payload.target != trackedPlayer)
                return;

            if (healthFill != null && payload.maxHealth > 0)
            {
                float percent = (payload.currentHealth / payload.maxHealth) * 100f;
                healthFill.style.width = Length.Percent(percent);
            }
            if (healthLabel != null)
            {
                healthLabel.text = Mathf.Max(0, (int)payload.currentHealth).ToString();
            }
        }

        /// <summary>
        /// Asigna dinámicamente el jugador que se está trackeando.
        /// </summary>
        public void SetTrackedPlayer(GameObject player)
        {
            trackedPlayer = player;
            var health = player.GetComponent<HealthComponent>();
            if (health != null)
            {
                float percent = (health.CurrentHealth / health.maxHealth) * 100f;
                if (healthFill != null) healthFill.style.width = Length.Percent(percent);
                if (healthLabel != null) healthLabel.text = Mathf.Max(0, (int)health.CurrentHealth).ToString();
            }
        }
    }
}
