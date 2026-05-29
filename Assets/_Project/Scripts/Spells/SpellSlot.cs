using UnityEngine;

namespace AnimalMagicRoyale.Spells
{
    [System.Serializable]
    public class SpellSlot
    {
        public SpellData spellData;
        public float lastCastTime = -100f;

        public bool IsEmpty => spellData == null;

        public bool IsOnCooldown
        {
            get
            {
                if (IsEmpty) return false;
                return Time.time - lastCastTime < spellData.cooldown;
            }
        }

        public float CooldownRemaining
        {
            get
            {
                if (IsEmpty || !IsOnCooldown) return 0f;
                return spellData.cooldown - (Time.time - lastCastTime);
            }
        }
    }
}
