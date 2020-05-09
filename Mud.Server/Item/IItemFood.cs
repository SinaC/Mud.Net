namespace Mud.Server.Item
{
    public interface IItemFood : IItem
    {
        int FullHour { get; }

        int HungerHour { get; }

        bool IsPoisoned { get; }

        void Poison();
        void Cure();
    }
}
