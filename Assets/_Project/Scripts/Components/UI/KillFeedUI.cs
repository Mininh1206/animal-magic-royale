using UnityEngine;
using AnimalMagicRoyale.Core;
using TMPro;
using System.Collections;

namespace AnimalMagicRoyale.Components.UI
{
    public class KillFeedUI : MonoBehaviour
    {
        [SerializeField] private GameObject bannerPanel;
        [SerializeField] private TextMeshProUGUI killText;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private PlayerEliminatedEvent onPlayerEliminated;
        
        private Coroutine hideCoroutine;

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

        private void Start()
        {
            if (bannerPanel != null)
            {
                bannerPanel.SetActive(false);
            }
        }

        private void HandlePlayerEliminated(PlayerEliminatedPayload payload)
        {
            Debug.Log($"[KillFeedUI] HandlePlayerEliminated called for {payload.eliminated?.name}");
            if (bannerPanel != null && killText != null)
            {
                string playerName = payload.eliminated != null ? payload.eliminated.name : "Un jugador";
                string outputText = "";
                if (payload.killer != null)
                {
                    outputText = $"{payload.killer.name} ha eliminado a {playerName}";
                }
                else
                {
                    outputText = $"{playerName} ha sido eliminado";
                }

                killText.text = outputText;
                
                bannerPanel.SetActive(true);
                Debug.Log($"[KillFeedUI] Banner activated with text: {killText.text}");
                
                if (hideCoroutine != null)
                {
                    StopCoroutine(hideCoroutine);
                }
                hideCoroutine = StartCoroutine(HideBannerAfterDelay());
            }
        }
        
        private IEnumerator HideBannerAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);
            if (bannerPanel != null)
            {
                bannerPanel.SetActive(false);
            }
        }
    }
}
