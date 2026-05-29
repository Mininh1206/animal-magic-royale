using UnityEngine;
using UnityEngine.UI;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Components.UI
{
    /// <summary>
    /// Conecta la barra de vida (Image tipo Filled) con el HealthComponent del jugador
    /// a través del evento HealthChangedEvent del EventBus.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image healthFillImage;
        [SerializeField] private GameObject trackedPlayer;

        [Header("Events")]
        [SerializeField] private HealthChangedEvent onHealthChanged;

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

        private void Start()
        {
            // Inicializar al 100%
            if (healthFillImage != null)
                healthFillImage.fillAmount = 1f;
        }

        private void HandleHealthChanged(HealthChangedPayload payload)
        {
            // Solo actualizar si es nuestro jugador (o si no hay filtro)
            if (trackedPlayer != null && payload.target != trackedPlayer)
                return;

            if (healthFillImage != null && payload.maxHealth > 0)
            {
                healthFillImage.fillAmount = payload.currentHealth / payload.maxHealth;
            }
        }

        /// <summary>
        /// Asigna dinámicamente el jugador que se está trackeando.
        /// </summary>
        public void SetTrackedPlayer(GameObject player)
        {
            trackedPlayer = player;
            var health = player.GetComponent<HealthComponent>();
            if (health != null && healthFillImage != null)
            {
                healthFillImage.fillAmount = health.CurrentHealth / health.maxHealth;
            }
        }
    }
}
