using UnityEngine;

namespace AnimalMagicRoyale.Player
{
    /// <summary>
    /// Calcula el punto de impacto y dirección de disparo usando un raycast
    /// desde el centro de la pantalla (cámara principal).
    /// </summary>
    public static class AimHelper
    {
        public static Vector3 GetAimDirection(Vector3 firePointPosition, GameObject caster, float maxDistance = 200f, LayerMask? hitMask = null)
        {
            UnityEngine.Camera cam = UnityEngine.Camera.main;
            if (cam == null) return Vector3.forward;
            
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            Vector3 targetPoint = ray.origin + ray.direction * maxDistance;
            
            RaycastHit[] hits;
            if (hitMask.HasValue)
            {
                hits = Physics.RaycastAll(ray, maxDistance, hitMask.Value);
            }
            else
            {
                // Ignite triggers if needed? RaycastAll by default uses Physics settings, which might hit triggers.
                hits = Physics.RaycastAll(ray, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            }
            
            // Ordenar por distancia
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            
            foreach (var hit in hits)
            {
                // Ignorar el propio cuerpo del tirador
                if (caster != null && hit.collider.transform.root == caster.transform.root)
                {
                    continue;
                }

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
