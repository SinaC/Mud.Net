﻿namespace Mud.POC.Affects
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
