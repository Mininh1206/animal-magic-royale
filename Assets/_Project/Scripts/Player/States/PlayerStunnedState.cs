using UnityEngine;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Player
{
    public class PlayerStunnedState : State
    {
        private PlayerController player;
        private float duration;
        private float startTime;

        public PlayerStunnedState(PlayerController player, StateMachine stateMachine) : base(stateMachine)
        {
            this.player = player;
        }

        public void SetDuration(float stunDuration)
        {
            duration = stunDuration;
        }

        public override void Enter()
        {
            startTime = Time.time;
        }

        public override void Update()
        {
            if (Time.time - startTime >= duration)
            {
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }
}
