using Mud.Common.Attributes;
using Mud.Domain.SerializationData;
using Mud.Server.Domain;

namespace Mud.Server.Affects.Room;

[JsonBaseType(typeof(AffectDataBase), "roomSourceRate")]
public class RoomResourceRateAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; }

    public required int Modifier { get; set; }
}
