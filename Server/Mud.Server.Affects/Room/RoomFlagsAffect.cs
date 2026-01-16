using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Room;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Affects.Room;

[Affect("RoomFlagsAffect", typeof(RoomFlagsAffectData))]
public class RoomFlagsAffect : FlagsAffectBase<IRoomFlags>, IRoomFlagsAffect
{
    protected override string Target => "Room flags";

    public void Initialize(RoomFlagsAffectData data)
    {
        Operator = data.Operator;
        Modifier = new RoomFlags(data.Modifier);
    }

    public void Apply(IRoom room)
    {
        room.ApplyAffect(this);
    }

    public override AffectDataBase MapAffectData()
    {
        return new RoomFlagsAffectData
        {
            Operator = Operator,
            Modifier = Modifier.Serialize()
        };
    }
}
