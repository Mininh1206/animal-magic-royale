using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

namespace AnimalMagicRoyale.Player
{
    /// <summary>
    /// Alimenta el eje vertical (pitch) del PanTilt de Cinemachine
    /// con el delta del ratón. El eje horizontal (yaw) lo gestiona
    /// PlayerController rotando al jugador directamente.
    /// </summary>
    [RequireComponent(typeof(CinemachinePanTilt))]
    public class CinemachinePOVInput : MonoBehaviour
    {
        [Tooltip("Sensibilidad vertical de la cámara")]
        public float verticalSensitivity = 15f;

        [Tooltip("Ángulo mínimo de pitch (mirar arriba)")]
        public float minPitch = -20f;

        [Tooltip("Ángulo máximo de pitch (mirar abajo)")]
        public float maxPitch = 60f;

        private CinemachinePanTilt panTilt;

        private void Awake()
        {
            panTilt = GetComponent<CinemachinePanTilt>();
        }

        private void Update()
        {
            if (panTilt == null || Mouse.current == null) return;

            if (AnimalMagicRoyale.Components.UI.SettingsManager.Instance != null && 
                AnimalMagicRoyale.Components.UI.SettingsManager.Instance.IsOpen)
            {
                return;
            }

            if (AnimalMagicRoyale.Core.GameManager.Instance != null && 
                AnimalMagicRoyale.Core.GameManager.Instance.StateMachine.CurrentState is AnimalMagicRoyale.Core.GameOverState)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            // Solo aplicar el eje vertical; el horizontal lo maneja PlayerController
            float pitchDelta = -mouseDelta.y * verticalSensitivity * Time.deltaTime;
            float newTilt = panTilt.TiltAxis.Value + pitchDelta;
            panTilt.TiltAxis.Value = Mathf.Clamp(newTilt, minPitch, maxPitch);

            // Forzar Pan a 0 para que no haya yaw duplicado
            panTilt.PanAxis.Value = 0f;
        }
    }
}
