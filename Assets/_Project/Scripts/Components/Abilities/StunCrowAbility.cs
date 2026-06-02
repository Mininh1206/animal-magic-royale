using UnityEngine;
using AnimalMagicRoyale.Player;
using AnimalMagicRoyale.Spells.Effects;

namespace AnimalMagicRoyale.Components.Abilities
{
    [CreateAssetMenu(fileName = "StunCrowAbility", menuName = "Animal Magic Royale/Abilities/Stun Crow")]
    public class StunCrowAbility : SpecialAbility
    {
        public float radius = 5f;
        public float stunDuration = 1.5f;
        public LayerMask targetLayer;

        public override void Activate(GameObject owner)
        {
            // Debug.Log($"{owner.name} activated Stun Crow! (AoE Stun)");
            Collider[] hits = Physics.OverlapSphere(owner.transform.position, radius, targetLayer);
            foreach (var hit in hits)
            {
                if (hit.gameObject != owner)
                {
                    var player = hit.GetComponent<PlayerController>();
                    if (player != null && player.StunnedState != null)
                    {
                        player.StunnedState.SetDuration(stunDuration);
                        player.StateMachine.ChangeState(player.StunnedState);
                    }
                }
            }
        }
    }
}
