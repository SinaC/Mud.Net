using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Affects.Room;

[JsonBaseType(typeof(AffectDataBase), "roomSourceRate")]
public class RoomResourceRateAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; }

    public required int Modifier { get; set; }
}
