namespace Mud.Server.Item
{
    public interface IItemLight : IItem, IEquipable
    {
        int TimeLeft { get; }
        void Consume();
    }
}
