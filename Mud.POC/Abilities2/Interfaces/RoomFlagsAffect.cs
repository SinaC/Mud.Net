﻿using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.Interfaces
{
    public class RoomFlagsAffect : FlagAffectBase<RoomFlags>, IRoomAffect
    {
        public void Apply(IRoom room)
        {
            throw new System.NotImplementedException();
        }
    }

}