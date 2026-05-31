using UnityEngine;
using AnimalMagicRoyale.Core;
using System.Collections;
using UnityEngine.UIElements;

namespace AnimalMagicRoyale.Components.UI
{
    public class KillFeedUI : MonoBehaviour
    {
        private VisualElement feedContainer;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private PlayerEliminatedEvent onPlayerEliminated;
        
        public void Initialize(VisualElement root)
        {
            feedContainer = root.Q<VisualElement>("killfeed-container");
            if (feedContainer != null)
            {
                feedContainer.Clear();
            }
        }

        private void Awake()
        {
            if (onPlayerEliminated == null) onPlayerEliminated = Resources.Load<PlayerEliminatedEvent>("Events/PlayerEliminatedEvent");
        }

        private void OnEnable()
        {
            if (onPlayerEliminated != null)
                onPlayerEliminated.RegisterListener(HandlePlayerEliminated);
        }
        
        private void OnDisable()
        {
            if (onPlayerEliminated != null)
                onPlayerEliminated.UnregisterListener(HandlePlayerEliminated);
        }

        private void HandlePlayerEliminated(PlayerEliminatedPayload payload)
        {
            if (feedContainer != null)
            {
                string victimName = payload.eliminated != null ? payload.eliminated.name : "Un jugador";
                
                var killItem = new VisualElement();
                killItem.AddToClassList("killfeed-item");
                
                if (payload.killer != null && payload.killer != payload.eliminated)
                {
                    bool isZone = payload.killer.GetComponent<AnimalMagicRoyale.Core.ZoneManager>() != null;
                    if (isZone)
                    {
                        var zoneLabel = new Label("La Zona");
                        zoneLabel.AddToClassList("killfeed-text");
                        zoneLabel.AddToClassList("killfeed-text-killer");
                        zoneLabel.style.color = new StyleColor(new Color(1f, 0.2f, 0.2f)); // Rojo zona
                        killItem.Add(zoneLabel);

                        var icon = new VisualElement();
                        icon.AddToClassList("killfeed-icon");
                        killItem.Add(icon);
                    }
                    else
                    {
                        string killerName = payload.killer.name;
                        var killerLabel = new Label(killerName);
                        killerLabel.AddToClassList("killfeed-text");
                        killerLabel.AddToClassList("killfeed-text-killer");
                        killItem.Add(killerLabel);

                        var icon = new VisualElement();
                        icon.AddToClassList("killfeed-icon");
                        killItem.Add(icon);
                    }
                }

                var victimLabel = new Label(victimName);
                victimLabel.AddToClassList("killfeed-text");
                killItem.Add(victimLabel);
                
                feedContainer.Add(killItem);
                
                StartCoroutine(FadeAndRemove(killItem));
            }
        }
        
        private IEnumerator FadeAndRemove(VisualElement item)
        {
            yield return new WaitForSeconds(displayDuration);
            
            if (item != null)
            {
                item.style.opacity = 0f;
                yield return new WaitForSeconds(fadeDuration);
                
                if (feedContainer != null && feedContainer.Contains(item))
                {
                    feedContainer.Remove(item);
                }
            }
        }
    }
}
