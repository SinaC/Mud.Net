using Mud.Domain;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Affects
{
    public class RoomFlagsAffect : FlagsAffectBase<IRoomFlags, IRoomFlagValues>, IRoomFlagsAffect
    {
        protected override string Target => "Room flags";

        // TODO: no serialization RoomFlagsAffectData doesn't exist

        public void Apply(IRoom room)
        {
            room.ApplyAffect(this);
        }

        public override AffectDataBase MapAffectData()
        {
            throw new System.NotImplementedException();
        }
    }

}
