namespace Mud.Server.Item
{
    public interface IItemDrinkContainer : IItemDrinkable
    {
        int MaxLiquidAmount { get; }
        int CurrentLiquidAmount { get; }

        void Poison();
        void Cure();
        void Fill(string liquidName);
        void Empty();
    }
}
