using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public enum AvatarCreationStates
    {
        // TODO
        RaceChoice,
        ClassChoice,
        AvatarCreated
    }
    public class AvatarCreationStateMachine : InputTrapBase<IPlayer, AvatarCreationStates>
    {
        public override bool IsFinalStateReached
        {
            get { return State == AvatarCreationStates.AvatarCreated; }
        }

        // TODO
    }
}
