using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Information
{
    // 0/ if sleeping/blind/room is dark
    // 1/ else if no parameter, look in room
    // 2/ else if 1st parameter is 'in' or 'on', search item (matching 2nd parameter) in the room, then inventory, then equipment, and display its content
    // 3/ else if a character can be found in room (matching 1st parameter), display character info
    // 4/ else if an item can be found in inventory+room (matching 1st parameter), display item description or extra description
    // 5/ else, if an extra description can be found in room (matching 1st parameter), display it
    // 6/ else, if 1st parameter is a direction, display if there is an exit/door
    [CharacterCommand("look", "Information", Priority = 0, MinPosition = Positions.Resting)]
    [Syntax(
        "[cmd]",
        "[cmd] in <container>",
        "[cmd] in <corpse>",
        "[cmd] <character>",
        "[cmd] <item>",
        "[cmd] <keyword>",
        "[cmd] <direction>")]
    public class Look : CharacterGameAction
    {
        public bool DisplayRoom { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            // 0: sleeping/blind/dark room
            if (Actor.Position < Positions.Sleeping)
                return "You can't see anything but stars!";
            if (Actor.Position == Positions.Sleeping)
                return "You can't see anything, you're sleeping!";
            if (Actor.CharacterFlags.HasFlag(CharacterFlags.Blind))
                return "You can't see a thing!";

            if (actionInput.Parameters.Length == 0)
            {
                DisplayRoom = true;
                return null;
            }

            return StringHelpers.ItemNotFound;
        }

        public override void Execute(IActionInput actionInput)
        {
            
        }
    }
}
