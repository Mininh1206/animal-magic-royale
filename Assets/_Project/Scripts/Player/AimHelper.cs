using UnityEngine;

namespace AnimalMagicRoyale.Player
{
    /// <summary>
    /// Calcula el punto de impacto y dirección de disparo usando un raycast
    /// desde el centro de la pantalla (cámara principal).
    /// </summary>
    public static class AimHelper
    {
        public static Vector3 GetAimDirection(Vector3 firePointPosition, float maxDistance = 200f, LayerMask? hitMask = null)
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            if (cam == null) return Vector3.forward;
            
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            Vector3 targetPoint = ray.origin + ray.direction * maxDistance;
            
            // Usamos RaycastAll para ignorar impactos que estén detrás del firePoint (como la espalda del jugador)
            RaycastHit[] hits;
            if (hitMask.HasValue)
            {
                hits = Physics.RaycastAll(ray, maxDistance, hitMask.Value);
            }
            else
            {
                hits = Physics.RaycastAll(ray, maxDistance);
            }
            
            // Ordenar por distancia
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            
            foreach (var hit in hits)
            {
                // Solo nos interesan los impactos que estén por delante del firePoint
                // (relativo a la dirección de la cámara)
                if (Vector3.Dot(cam.transform.forward, hit.point - firePointPosition) > 0)
                {
                    targetPoint = hit.point;
                    break;
                }
            }
            
            return (targetPoint - firePointPosition).normalized;
        }
    }
}
