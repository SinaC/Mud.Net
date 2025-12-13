using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Affects.Room;

[JsonPolymorphism(typeof(AffectDataBase), "roomFlags")]
public class RoomFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required IRoomFlags Modifier { get; set; }
}
