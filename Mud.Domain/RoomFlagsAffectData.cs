using Mud.Server.Flags.Interfaces;

namespace Mud.Domain;

public class RoomFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required IRoomFlags Modifier { get; set; }
}
