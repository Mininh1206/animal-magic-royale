namespace AnimalMagicRoyale.AI
{
    public enum NodeStatus
    {
        Success,
        Failure,
        Running
    }

    public abstract class BTNode
    {
        public abstract NodeStatus Tick(BotContext context);
    }
}
