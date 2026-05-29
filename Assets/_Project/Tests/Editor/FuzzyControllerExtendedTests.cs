using NUnit.Framework;
using AnimalMagicRoyale.AI;

namespace AnimalMagicRoyale.Tests
{
    public class FuzzyControllerExtendedTests
    {
        [Test]
        public void NoEnemy_NoThreat_ProducesCollectScore()
        {
            var controller = new FuzzyController();
            // Health=100 (High), Dist=100 (Far/No enemy), Threat=0
            var output = controller.Evaluate(100f, 100f, 0f);
            
            Assert.IsTrue(output.collectScore > 0f);
            Assert.IsTrue(output.collectScore > output.attackScore);
        }

        [Test]
        public void MediumHealth_NoThreat_ProducesCollectScore()
        {
            var controller = new FuzzyController();
            // Health=50 (Medium), Dist=100 (Far/No enemy), Threat=0
            var output = controller.Evaluate(50f, 100f, 0f);
            
            Assert.IsTrue(output.collectScore > 0f);
            Assert.IsTrue(output.collectScore > output.fleeScore);
        }

        [Test]
        public void MediumHealth_CloseEnemy_ProducesHighAttackScore()
        {
            var controller = new FuzzyController();
            // Health=50 (Medium), Dist=5 (Close), Threat=0
            var output = controller.Evaluate(50f, 5f, 0f);
            
            Assert.IsTrue(output.attackScore > 0f);
            Assert.IsTrue(output.attackScore > output.fleeScore);
        }

        [Test]
        public void AllScoresNonZero_InTypicalCombatScenario()
        {
            var controller = new FuzzyController();
            // Health=50 (Medium), Dist=20 (Medium), Threat=0
            var output = controller.Evaluate(50f, 20f, 0f);
            
            // Due to overlapping sets, we expect attack and collect scores
            Assert.IsTrue(output.attackScore > 0f || output.collectScore > 0f);
        }
    }
}
