namespace Mud.Server.Item
{
    public interface IItemLight : IItem
    {
        int TimeLeft { get; }
        void Consume();
    }
}
