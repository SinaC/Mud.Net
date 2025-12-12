using Mud.Domain;
using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Affects.Room;

[JsonPolymorphism(typeof(AffectDataBase), "roomHealRate")]
public class RoomHealRateAffectData : AffectDataBase
{
    public required AffectOperators Operator { get; set; }

    public required int Modifier { get; set; }
}
