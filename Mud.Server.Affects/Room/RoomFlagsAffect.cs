using Mud.Domain;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Room;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Affects.Room;

[Affect("RoomFlagsAffect", typeof(RoomFlagsAffectData))]
public class RoomFlagsAffect : FlagsAffectBase<IRoomFlags, IRoomFlagValues>, IRoomFlagsAffect
{
    protected override string Target => "Room flags";

    public RoomFlagsAffect()
    {
    }

    public RoomFlagsAffect(RoomFlagsAffectData data)
    {
        Operator = data.Operator;
        Modifier = data.Modifier;
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
            Modifier = Modifier,
        };
    }
}
