using Mud.Domain.SerializationData;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Room;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Affects.Room;

[Affect("RoomFlagsAffect", typeof(RoomFlagsAffectData))]
public class RoomFlagsAffect : FlagsAffectBase<IRoomFlags, IRoomFlagValues>, IRoomFlagsAffect
{
    private IFlagFactory<IRoomFlags, IRoomFlagValues> FlagFactory { get; }

    public RoomFlagsAffect(IFlagFactory<IRoomFlags, IRoomFlagValues> flagFactory)
    {
        FlagFactory = flagFactory;
    }

    protected override string Target => "Room flags";


    public void Initialize(RoomFlagsAffectData data)
    {
        Operator = data.Operator;
        Modifier = FlagFactory.CreateInstance(data.Modifier);
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
