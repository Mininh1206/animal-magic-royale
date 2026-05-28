using NUnit.Framework;
using AnimalMagicRoyale.AI;
using System.Collections.Generic;

namespace AnimalMagicRoyale.Tests
{
    public class BehaviorTreeTests
    {
        [Test]
        public void BTCondition_EvaluatesPredicate()
        {
            var trueCondition = new BTCondition(ctx => true);
            var falseCondition = new BTCondition(ctx => false);
            var context = new BotContext();

            Assert.AreEqual(NodeStatus.Success, trueCondition.Tick(context));
            Assert.AreEqual(NodeStatus.Failure, falseCondition.Tick(context));
        }

        [Test]
        public void BTSelector_ReturnsSuccessOnFirstSuccess()
        {
            var child1 = new BTCondition(ctx => false);
            var child2 = new BTCondition(ctx => true);
            var child3 = new BTCondition(ctx => false);
            
            var selector = new BTSelector(new List<BTNode> { child1, child2, child3 });
            var context = new BotContext();

            Assert.AreEqual(NodeStatus.Success, selector.Tick(context));
        }

        [Test]
        public void BTSelector_ReturnsFailureIfAllFail()
        {
            var child1 = new BTCondition(ctx => false);
            var child2 = new BTCondition(ctx => false);
            
            var selector = new BTSelector(new List<BTNode> { child1, child2 });
            var context = new BotContext();

            Assert.AreEqual(NodeStatus.Failure, selector.Tick(context));
        }

        [Test]
        public void BTSequence_ReturnsFailureOnFirstFailure()
        {
            var child1 = new BTCondition(ctx => true);
            var child2 = new BTCondition(ctx => false);
            var child3 = new BTCondition(ctx => true);
            
            var sequence = new BTSequence(new List<BTNode> { child1, child2, child3 });
            var context = new BotContext();

            Assert.AreEqual(NodeStatus.Failure, sequence.Tick(context));
        }

        [Test]
        public void BTSequence_ReturnsSuccessIfAllSucceed()
        {
            var child1 = new BTCondition(ctx => true);
            var child2 = new BTCondition(ctx => true);
            
            var sequence = new BTSequence(new List<BTNode> { child1, child2 });
            var context = new BotContext();

            Assert.AreEqual(NodeStatus.Success, sequence.Tick(context));
        }
        
        [Test]
        public void BTAction_ExecutesAction()
        {
            bool executed = false;
            var action = new BTAction(ctx => 
            {
                executed = true;
                return NodeStatus.Running;
            });
            var context = new BotContext();
            
            var result = action.Tick(context);
            
            Assert.IsTrue(executed);
            Assert.AreEqual(NodeStatus.Running, result);
        }
    }
}
