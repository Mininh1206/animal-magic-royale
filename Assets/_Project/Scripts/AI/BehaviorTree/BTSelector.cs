using System.Collections.Generic;

namespace AnimalMagicRoyale.AI
{
    /// <summary>
    /// Executes children in order. Returns Success if any child succeeds.
    /// Returns Failure only if all children fail.
    /// </summary>
    public class BTSelector : BTNode
    {
        private List<BTNode> children;

        public BTSelector(List<BTNode> children)
        {
            this.children = children;
        }

        public override NodeStatus Tick(BotContext context)
        {
            foreach (var child in children)
            {
                var status = child.Tick(context);
                if (status != NodeStatus.Failure)
                {
                    return status;
                }
            }
            return NodeStatus.Failure;
        }
    }
}
