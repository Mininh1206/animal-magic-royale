using Unity.Services.Multiplayer;
using UnityEngine;

namespace AnimalMagicRoyale.Components
{
    public class AbilityHolder : MonoBehaviour
    {
        [SerializeField] private SpecialAbility ability;

        public SpecialAbility Ability => ability;

        private float lastUseTime = -Mathf.Infinity;

        public void Initialize(SpecialAbility specialAbility)
        {
            this.ability = specialAbility;
        }

        public bool IsReady => ability != null && ability.CanActivate(lastUseTime);
        public float TotalCooldown => ability != null ? ability.cooldown : 0f;

        public bool TryActivate()
        {
            Debug.Log($"[AbilityHolder] TryActivate called on {gameObject.name}. IsReady: {IsReady}, ability is null: {ability == null}");
            if (IsReady)
            {
                Debug.Log($"[AbilityHolder] Activating ability {ability.abilityName} on {gameObject.name}");
                ability.Activate(gameObject);
                lastUseTime = Time.time;
                return true;
            }
            if (ability != null && !ability.CanActivate(lastUseTime))
            {
                Debug.LogWarning($"[AbilityHolder] Ability {ability.abilityName} is on cooldown! {GetCooldownRemaining()}s left.");
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
