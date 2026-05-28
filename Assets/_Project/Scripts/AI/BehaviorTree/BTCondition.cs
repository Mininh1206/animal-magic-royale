using System;

namespace AnimalMagicRoyale.AI
{
    public class BTCondition : BTNode
    {
        private Func<BotContext, bool> predicate;

        public BTCondition(Func<BotContext, bool> predicate)
        {
            this.predicate = predicate;
        }

        public override NodeStatus Tick(BotContext context)
        {
            if (predicate(context))
            {
                return NodeStatus.Success;
            }
            return NodeStatus.Failure;
        }
    }
}
