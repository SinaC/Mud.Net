namespace Mud.Server.Item
{
    public interface IItemFood : IItemPoisonable
    {
        int FullHours { get; }

        int HungerHours { get; }

        void SetHours(int fullHours, int hungerHours);
    }
}
