using System;

namespace AnimalMagicRoyale.AI
{
    public class BTAction : BTNode
    {
        private Func<BotContext, NodeStatus> action;

        public BTAction(Func<BotContext, NodeStatus> action)
        {
            this.action = action;
        }

        public override NodeStatus Tick(BotContext context)
        {
            return action(context);
        }
    }
}
