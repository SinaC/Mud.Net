namespace Mud.Server.Interfaces.Item
{
    public interface IItemPoisonable : IItem
    {
        bool IsPoisoned { get; }

        void Poison();
        void Cure();
    }
}
