using Mud.Server.Flags.Interfaces;

namespace Mud.Domain
{
    public class RoomFlagsAffectData : AffectDataBase
    {
        public AffectOperators Operator { get; set; } // Add and Or are identical

        public IRoomFlags Modifier { get; set; }
    }
}
