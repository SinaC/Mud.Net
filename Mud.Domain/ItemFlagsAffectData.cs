using Mud.Server.Flags.Interfaces;

namespace Mud.Domain;

public class ItemFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required IItemFlags Modifier { get; set; }
}
