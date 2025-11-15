using Mud.Server.Flags.Interfaces;

namespace Mud.Domain;

public class ItemWeaponFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required IWeaponFlags Modifier { get; set; }
}
