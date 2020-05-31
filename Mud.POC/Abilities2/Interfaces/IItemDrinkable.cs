namespace Mud.POC.Abilities2.Interfaces
{
    public interface IItemDrinkable : IItem
    {
        string LiquidName { get; }
        int LiquidLeft { get; }
        bool IsEmpty { get; }
        int LiquidAmountMultiplier { get; }

        void Drink(int amount);
    }
}
