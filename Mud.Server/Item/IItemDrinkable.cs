namespace Mud.Server.Item
{
    public interface IItemDrinkable : IItem
    {
        string LiquidName { get; }
        int LiquidLeft { get; }
        bool IsEmpty { get; }
        bool IsPoisoned { get; }
        int LiquidAmountMultiplier { get; }

        void Drink(int amount);
    }
}
