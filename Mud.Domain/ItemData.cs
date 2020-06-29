using Mud.Server.Flags.Interfaces;

namespace Mud.Domain
{
    public class ItemData
    {
        public int ItemId { get; set; }

        public int Level { get; set; }

        public int DecayPulseLeft { get; set; }

        public IItemFlags ItemFlags { get; set; }

        public AuraData[] Auras { get; set; }
    }
}
