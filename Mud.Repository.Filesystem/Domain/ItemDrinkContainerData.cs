namespace Mud.Repository.Filesystem.Domain
{
    public class ItemDrinkContainerData : ItemData
    {
        public int MaxLiquidAmount { get; set; }

        public int CurrentLiquidAmount { get; set; }

        public string LiquidName { get; set; }

        public bool IsPoisoned { get; set; }
    }
}
