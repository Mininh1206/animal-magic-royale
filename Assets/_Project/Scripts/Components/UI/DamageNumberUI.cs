using UnityEngine;
using UnityEngine.UIElements;
using AnimalMagicRoyale.Core;
using System.Collections.Generic;

namespace AnimalMagicRoyale.Components.UI
{
    public class DamageNumberUI : MonoBehaviour
    {
        [SerializeField] private HealthChangedEvent onHealthChanged;
        
        private VisualElement root;
        private List<DamageLabel> activeLabels = new List<DamageLabel>();

        private class DamageLabel
        {
            public Label label;
            public Vector3 worldPos;
            public float spawnTime;
            public float duration = 1.0f;
        }

        private void Awake()
        {
            if (onHealthChanged == null) onHealthChanged = Resources.Load<HealthChangedEvent>("Events/HealthChangedEvent");
        }

        private void OnEnable()
        {
            if (onHealthChanged != null) onHealthChanged.RegisterListener(HandleHealthChanged);
        }

        private void OnDisable()
        {
            if (onHealthChanged != null) onHealthChanged.UnregisterListener(HandleHealthChanged);
        }

        public void Initialize(VisualElement rootElement)
        {
            this.root = rootElement;
        }

        private void HandleHealthChanged(HealthChangedPayload payload)
        {
            // Only show negative delta (damage)
            if (payload.delta >= 0 || root == null || payload.target == null) return;

            // Ignorar al jugador local (no queremos ver los números del daño que recibimos nosotros mismos)
            if (payload.target.GetComponent<AnimalMagicRoyale.Player.PlayerInputHandler>() != null) return;

            // Comprobar si el jugador objetivo está visible para la cámara
            if (UnityEngine.Camera.main != null)
            {
                Vector3 targetPos = payload.target.transform.position;
                Vector3 screenPos = UnityEngine.Camera.main.WorldToScreenPoint(targetPos);
                
                // 1. Detrás de la cámara o muy lejos
                if (screenPos.z < 0 || screenPos.z > 80f) return;
                
                // 2. Fuera de los límites de la pantalla
                if (screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height) return;
                
                // 3. Comprobar línea de visión (oclusión por paredes)
                Vector3 camPos = UnityEngine.Camera.main.transform.position;
                Vector3 dir = (targetPos + Vector3.up * 1f) - camPos;
                if (Physics.Raycast(camPos, dir.normalized, out RaycastHit hit, dir.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    // Si choca con algo que no es el objetivo (ej. pared u otro objeto sólido), no mostramos el texto
                    if (hit.collider.gameObject != payload.target && !hit.collider.transform.IsChildOf(payload.target.transform))
                    {
                        return;
                    }
                }
            }

            // Damage style
            Color dmgColor = new Color(1f, 0.6f, 0f); // Orange
            if (Mathf.Abs(payload.delta) > 25f || payload.source == null) 
            {
                dmgColor = Color.red; // Critical or Zone damage
            }

            var label = new Label(Mathf.RoundToInt(Mathf.Abs(payload.delta)).ToString());
            label.style.position = Position.Absolute;
            label.style.color = dmgColor;
            label.style.fontSize = 28;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.textShadow = new TextShadow { color = Color.black, offset = new Vector2(2, 2), blurRadius = 1f };

            root.Add(label);

            var dmgLabel = new DamageLabel
            {
                label = label,
                worldPos = payload.target.transform.position + Vector3.up * 2f,
                spawnTime = Time.time
            };

            activeLabels.Add(dmgLabel);
            UpdateLabelPosition(dmgLabel);
        }

        private void Update()
        {
            if (root == null || UnityEngine.Camera.main == null) return;

            for (int i = activeLabels.Count - 1; i >= 0; i--)
            {
                var dmg = activeLabels[i];
                float t = (Time.time - dmg.spawnTime) / dmg.duration;

                if (t >= 1f)
                {
                    if (root.Contains(dmg.label)) root.Remove(dmg.label);
                    activeLabels.RemoveAt(i);
                    continue;
                }

                // Animate
                dmg.worldPos += Vector3.up * Time.deltaTime * 2.5f;
                UpdateLabelPosition(dmg);

                // Fade out
                dmg.label.style.opacity = 1f - (t * t); // ease-in fade
            }
        }

        private void UpdateLabelPosition(DamageLabel dmg)
        {
            if (UnityEngine.Camera.main == null || dmg.label.panel == null) return;
            
            Vector3 screenPos = UnityEngine.Camera.main.WorldToScreenPoint(dmg.worldPos);
            // Ocultar si está detrás de la cámara o muy fuera de la pantalla
            if (screenPos.z < 0 || screenPos.x < -100 || screenPos.x > Screen.width + 100 || screenPos.y < -100 || screenPos.y > Screen.height + 100)
            {
                dmg.label.style.display = DisplayStyle.None;
                return;
            }

            dmg.label.style.display = DisplayStyle.Flex;
            
            // Convert to panel coordinates
            float rootWidth = root.resolvedStyle.width;
            float rootHeight = root.resolvedStyle.height;
            
            if (rootWidth == 0 || rootHeight == 0)
            {
                rootWidth = Screen.width;
                rootHeight = Screen.height;
            }
            
            float x = (screenPos.x / Screen.width) * rootWidth;
            float y = ((Screen.height - screenPos.y) / Screen.height) * rootHeight;

            dmg.label.style.left = x;
            dmg.label.style.top = y;
        }
    }
}
