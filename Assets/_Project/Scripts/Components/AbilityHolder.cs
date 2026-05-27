using UnityEngine;

namespace AnimalMagicRoyale.Components
{
    public class AbilityHolder : MonoBehaviour
    {
        [SerializeField] private SpecialAbility ability;
        private float lastUseTime = -Mathf.Infinity;

        public bool IsReady => ability != null && ability.CanActivate(lastUseTime);

        public bool TryActivate()
        {
            if (IsReady)
            {
                ability.Activate(gameObject);
                lastUseTime = Time.time;
                return true;
            }
            return false;
        }

        public float GetCooldownRemaining()
        {
            if (ability == null) return 0f;
            float timePassed = Time.time - lastUseTime;
            if (timePassed >= ability.cooldown) return 0f;
            return ability.cooldown - timePassed;
        }
    }
}
