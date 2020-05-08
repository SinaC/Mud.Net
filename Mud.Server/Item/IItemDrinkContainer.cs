namespace Mud.Server.Item
{
    public interface IItemDrinkContainer : IItemDrinkable
    {
        int MaxLiquid { get; }

        void Poison();
        void Cure();
        void Fill(string liquidName, int amount);
        void Fill(int amount);
        void Pour();
    }
}
