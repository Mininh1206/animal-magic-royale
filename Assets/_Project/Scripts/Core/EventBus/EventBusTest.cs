using UnityEngine;

namespace AnimalMagicRoyale.Core
{
    /// <summary>
    /// Script de prueba temporal para verificar el funcionamiento del EventBus.
    /// </summary>
    public class EventBusTest : MonoBehaviour
    {
        [SerializeField] private IntEvent onPlayerDamaged;

        private void OnEnable()
        {
            if (onPlayerDamaged != null)
            {
                onPlayerDamaged.RegisterListener(HandlePlayerDamaged);
            }
        }

        private void OnDisable()
        {
            if (onPlayerDamaged != null)
            {
                onPlayerDamaged.UnregisterListener(HandlePlayerDamaged);
            }
        }

        private void Start()
        {
            // Debug.Log("[EventBusTest] Iniciando prueba...");
            if (onPlayerDamaged != null)
            {
                // Debug.Log("[EventBusTest] Lanzando evento OnPlayerDamaged con valor 25.");
                onPlayerDamaged.Raise(25);
            }
            else
            {
                // Debug.LogWarning("[EventBusTest] El evento OnPlayerDamaged no está asignado.");
            }
        }

        private void HandlePlayerDamaged(int damage)
        {
            // Debug.Log($"[EventBusTest] Evento recibido: Jugador dañado por {damage} puntos de vida.");
        }
    }
}
