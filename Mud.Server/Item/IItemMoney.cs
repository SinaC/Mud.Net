namespace Mud.Server.Item
{
    public interface IItemMoney : IItem
    {
        long SilverCoins { get; }
        long GoldCoins { get; }
    }
}
