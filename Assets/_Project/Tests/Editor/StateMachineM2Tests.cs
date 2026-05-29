using NUnit.Framework;
using UnityEngine;
using AnimalMagicRoyale.Player;
using AnimalMagicRoyale.Core;

namespace AnimalMagicRoyale.Tests
{
    public class StateMachineM2Tests
    {
        private GameObject go;
        private PlayerController player;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("Player");
            go.AddComponent<CharacterController>();
            player = go.AddComponent<PlayerController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void StunnedState_BlocksInput_And_ExitsAfterDuration()
        {
            player.StunnedState.SetDuration(0.1f);
            player.StateMachine.ChangeState(player.StunnedState);
            
            Assert.AreEqual(player.StunnedState, player.StateMachine.CurrentState);
            
            player.MoveInput = new Vector2(1, 0);
            
            player.StateMachine.Update();
            
            Assert.AreEqual(player.StunnedState, player.StateMachine.CurrentState);
        }
    }
}
