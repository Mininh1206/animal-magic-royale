using UnityEngine;

namespace AnimalMagicRoyale.Core
{
    [RequireComponent(typeof(Collider))]
    public class ZoneDamageColliderProxy : MonoBehaviour
    {
        private ZoneManager zoneManager;

        public void Initialize(ZoneManager manager)
        {
            zoneManager = manager;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (zoneManager != null)
            {
                zoneManager.HandleTriggerEnter(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (zoneManager != null)
            {
                zoneManager.HandleTriggerExit(other);
            }
        }
    }
}
