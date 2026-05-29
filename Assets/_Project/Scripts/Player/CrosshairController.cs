using UnityEngine;

namespace AnimalMagicRoyale.Player
{
    /// <summary>
    /// Muestra una cruceta centrada en la pantalla del jugador.
    /// </summary>
    public class CrosshairController : MonoBehaviour
    {
        [SerializeField] private RectTransform crosshairImage;
        
        private void Start()
        {
            if (crosshairImage != null)
            {
                // Centrar en pantalla
                crosshairImage.anchoredPosition = Vector2.zero;
            }
        }
    }
}
