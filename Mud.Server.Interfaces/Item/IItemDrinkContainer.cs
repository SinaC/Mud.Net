namespace Mud.Server.Interfaces.Item
{
    public interface IItemDrinkContainer : IItemDrinkable, IItemPoisonable
    {
        int MaxLiquid { get; }

        void Fill(string liquidName, int amount);
        void Fill(int amount);
        void Pour();
    }
}
