namespace Mud.Domain
{
    public class ItemData
    {
        public int ItemId { get; set; }

        public int DecayPulseLeft { get; set; }

        public ItemFlags ItemFlags { get; set; }

        //TODO: enchantments/auras
    }
}
