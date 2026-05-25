using UnityEngine;
using AnimalMagicRoyale.Player;

namespace AnimalMagicRoyale.Spells.Effects
{
    [CreateAssetMenu(fileName = "StunEffect", menuName = "Animal Magic Royale/Spells/Effects/Stun")]
    public class StunEffect : SpellEffect
    {
        public float duration;

        public override void Apply(GameObject caster, GameObject target)
        {
            if (target == null) return;
            
            var playerController = target.GetComponent<PlayerController>();
            if (playerController != null && playerController.StunnedState != null)
            {
                playerController.StunnedState.SetDuration(duration);
                playerController.StateMachine.ChangeState(playerController.StunnedState);
            }
        }
    }
}
