using UnityEngine;

namespace AnimalMagicRoyale.Components
{
    public abstract class SpecialAbility : ScriptableObject
    {
        public string abilityName;
        public float cooldown;
        public Sprite icon;

        public abstract void Activate(GameObject owner);
        
        public virtual bool CanActivate(float lastUseTime)
        {
            return Time.time - lastUseTime >= cooldown;
        }
    }
}
