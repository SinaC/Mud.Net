using Mud.Server.Flags.Interfaces;

namespace Mud.Domain
{
    public class ItemFlagsAffectData : AffectDataBase
    {
        public AffectOperators Operator { get; set; } // Add and Or are identical

        public IItemFlags Modifier { get; set; }
    }
}
