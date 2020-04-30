using Mud.Domain;

namespace Mud.Server.Aura
{
    public class RoomFlagsAffect : FlagAffectBase<RoomFlags>, IRoomAffect
    {
        protected override string Target => "Room flags";

        public void Apply(IRoom room)
        {
            room.ApplyAffect(this);
        }
    }

}
