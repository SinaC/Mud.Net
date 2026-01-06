using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain;

namespace Mud.Server.Affects.Room;

[JsonBaseType(typeof(AffectDataBase), "roomFlags")]
public class RoomFlagsAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; } // Add and Or are identical

    public required string Modifier { get; set; }
}
