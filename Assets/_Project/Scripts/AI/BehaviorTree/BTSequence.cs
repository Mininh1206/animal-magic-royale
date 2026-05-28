using System.Collections.Generic;

namespace AnimalMagicRoyale.AI
{
    /// <summary>
    /// Executes children in order. Returns Failure if any child fails.
    /// Returns Success only if all children succeed.
    /// </summary>
    public class BTSequence : BTNode
    {
        private List<BTNode> children;

        public BTSequence(List<BTNode> children)
        {
            this.children = children;
        }

        public override NodeStatus Tick(BotContext context)
        {
            bool anyRunning = false;
            foreach (var child in children)
            {
                var status = child.Tick(context);
                if (status == NodeStatus.Failure)
                {
                    return NodeStatus.Failure;
                }
                if (status == NodeStatus.Running)
                {
                    anyRunning = true;
                    // Usually sequences don't proceed past running nodes
                    return NodeStatus.Running;
                }
            }
            return anyRunning ? NodeStatus.Running : NodeStatus.Success;
        }
    }
}
