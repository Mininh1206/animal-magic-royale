using NUnit.Framework;
using AnimalMagicRoyale.AI;
using System.Collections.Generic;

namespace AnimalMagicRoyale.Tests
{
    public class FuzzyControllerTests
    {
        [Test]
        public void FuzzySet_Triangle_EvaluatesCorrectly()
        {
            var set = new FuzzySet("Triangle", 0, 50, 50, 100);
            Assert.AreEqual(0f, set.Evaluate(0));
            Assert.AreEqual(0.5f, set.Evaluate(25));
            Assert.AreEqual(1f, set.Evaluate(50));
            Assert.AreEqual(0.5f, set.Evaluate(75));
            Assert.AreEqual(0f, set.Evaluate(100));
        }

        [Test]
        public void FuzzySet_Trapezoid_EvaluatesCorrectly()
        {
            var set = new FuzzySet("Trapezoid", 0, 20, 80, 100);
            Assert.AreEqual(0f, set.Evaluate(0));
            Assert.AreEqual(0.5f, set.Evaluate(10));
            Assert.AreEqual(1f, set.Evaluate(20));
            Assert.AreEqual(1f, set.Evaluate(50));
            Assert.AreEqual(1f, set.Evaluate(80));
            Assert.AreEqual(0.5f, set.Evaluate(90));
            Assert.AreEqual(0f, set.Evaluate(100));
        }

        [Test]
        public void FuzzyRule_Evaluate_ReturnsMinMembership()
        {
            var rule = new FuzzyRule()
                .AddCondition("Health", new FuzzySet("Low", 0, 0, 50, 100))
                .AddCondition("Distance", new FuzzySet("Close", 0, 0, 10, 20));
            
            var inputs = new Dictionary<string, float>
            {
                { "Health", 25 }, // Membership: 0.75
                { "Distance", 15 } // Membership: 0.5
            };

            float activation = rule.Evaluate(inputs);
            Assert.AreEqual(0.5f, activation); // Min(0.75, 0.5)
        }

        [Test]
        public void FuzzyController_LowHealth_HighThreat_Flees()
        {
            var controller = new FuzzyController();
            var output = controller.Evaluate(10f, 2f, 0.9f); // Health=10, Dist=2, Threat=0.9
            
            Assert.IsTrue(output.fleeScore > output.attackScore);
            Assert.IsTrue(output.fleeScore > 0.8f); // Strong flee score expected
        }

        [Test]
        public void FuzzyController_HighHealth_LowThreat_CloseEnemy_Attacks()
        {
            var controller = new FuzzyController();
            var output = controller.Evaluate(90f, 5f, 0.1f); // Health=90, Dist=5, Threat=0.1
            
            Assert.IsTrue(output.attackScore > output.fleeScore);
            Assert.IsTrue(output.attackScore > 0.8f); // Strong attack score expected
        }
    }
}
