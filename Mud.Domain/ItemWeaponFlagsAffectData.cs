using Mud.Server.Flags.Interfaces;

namespace Mud.Domain
{
    public class ItemWeaponFlagsAffectData : AffectDataBase
    {
        public AffectOperators Operator { get; set; } // Add and Or are identical

        public IWeaponFlags Modifier { get; set; }
    }
}
