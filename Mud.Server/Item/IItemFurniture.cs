namespace Mud.Server.Item
{
    public interface IItemFurniture : IItem
    {
        int MaxPeople { get; }
        int MaxWeight { get; }
        // TODO: flags (see tables.C furniture_flags)
        int HealBonus { get; }
        int ResourceBonus { get; }
    }
}
