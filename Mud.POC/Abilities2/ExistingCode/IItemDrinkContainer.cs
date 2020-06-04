namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IItemDrinkContainer : IItemDrinkable, IItemPoisonable
    {
        int MaxLiquid { get; }

        void Fill(string liquidName, int amount);
        void Fill(int amount);
        void Pour();
    }
}
