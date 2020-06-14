using Mud.Domain;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Affects
{
    public class RoomFlagsAffect : FlagAffectBase<RoomFlags>, IRoomFlagsAffect
    {
        protected override string Target => "Room flags";

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
