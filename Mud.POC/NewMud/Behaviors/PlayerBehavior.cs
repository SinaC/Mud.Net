using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.POC.NewMud.Behaviors
{
    // The behavior for players.
    public class PlayerBehavior : Behavior
    {
        // PlayerData: Id, UserName, DisplayName, ...

        public GameRace Race { get; set; }

        public GameGender Gender { get; set; }

        // TODO Load/Save
        // TODO Login/Logout/SetPassword

        // TODO Dispose
    }
}
