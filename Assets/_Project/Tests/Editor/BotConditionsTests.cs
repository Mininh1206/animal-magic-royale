using NUnit.Framework;
using UnityEngine;
using AnimalMagicRoyale.AI;

namespace AnimalMagicRoyale.Tests
{
    public class BotConditionsTests
    {
        private BotController CreateMockBot()
        {
            var go = new GameObject("MockBot");
            // Since BotController requires NavMeshAgent, HealthComponent, SpellInventory, and AISensorSystem
            // we'd need to mock those. For BotConditions, usually we only test the conditions against the Context.
            return go.AddComponent<BotController>();
        }

        [Test]
        public void ShouldAttack_WithEnemyAndHighAttackScore_ReturnsSuccess()
        {
            var context = new BotContext
            {
                NearestEnemy = new GameObject().transform,
                FuzzyResult = new FuzzyOutput(0.8f, 0.1f, 0.1f)
            };

            var condition = BotConditions.ShouldAttack();
            Assert.AreEqual(NodeStatus.Success, condition.Tick(context));
        }

        [Test]
        public void ShouldAttack_WithoutEnemy_ReturnsFailure()
        {
            var context = new BotContext
            {
                NearestEnemy = null,
                FuzzyResult = new FuzzyOutput(0.8f, 0.1f, 0.1f)
            };

            var condition = BotConditions.ShouldAttack();
            Assert.AreEqual(NodeStatus.Failure, condition.Tick(context));
        }

        [Test]
        public void ShouldFlee_WithHighFleeScore_ReturnsSuccess()
        {
            var context = new BotContext
            {
                FuzzyResult = new FuzzyOutput(0.1f, 0.9f, 0.1f)
            };

            var condition = BotConditions.ShouldFlee();
            Assert.AreEqual(NodeStatus.Success, condition.Tick(context));
        }

        [Test]
        public void ShouldCollect_WithLootAndHighCollectScore_ReturnsSuccess()
        {
            var context = new BotContext
            {
                BestPhysicalLoot = new GameObject().transform,
                FuzzyResult = new FuzzyOutput(0.1f, 0.1f, 0.8f)
            };

            var condition = BotConditions.ShouldCollect();
            Assert.AreEqual(NodeStatus.Success, condition.Tick(context));
        }
    }
}
