using Unity.Services.Multiplayer;
using UnityEngine;

namespace AnimalMagicRoyale.Components
{
    public class AbilityHolder : MonoBehaviour
    {
        [SerializeField] private SpecialAbility ability;

        public SpecialAbility Ability => ability;

        private float lastUseTime = -Mathf.Infinity;

        public bool IsReady => ability != null && ability.CanActivate(lastUseTime);
        public float TotalCooldown => ability != null ? ability.cooldown : 0f;

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
