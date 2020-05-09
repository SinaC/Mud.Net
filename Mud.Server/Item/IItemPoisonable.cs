namespace Mud.Server.Item
{
    public interface IItemPoisonable : IItem
    {
        bool IsPoisoned { get; }

        void Poison();
        void Cure();
    }
}
