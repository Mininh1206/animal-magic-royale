using UnityEngine;

namespace AnimalMagicRoyale.Components
{
    public class ZoneDamageTracker : MonoBehaviour
    {
        public float timeOutsideZone { get; private set; }
        public bool isOutside { get; private set; }

        public void UpdateZoneStatus(bool insideZone)
        {
            if (insideZone)
            {
                isOutside = false;
                timeOutsideZone = 0f;
            }
            else
            {
                isOutside = true;
            }
        }

        private void Update()
        {
            if (isOutside)
            {
                timeOutsideZone += Time.deltaTime;
            }
        }

        public float GetDamageMultiplier(float incrementRate)
        {
            if (!isOutside) return 1f;
            return 1f + (timeOutsideZone * incrementRate);
        }
    }
}
