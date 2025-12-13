namespace Mud.Server.Interfaces.Item;

public interface IItemMoney : IItem
{
    long SilverCoins { get; }
    long GoldCoins { get; }
}
