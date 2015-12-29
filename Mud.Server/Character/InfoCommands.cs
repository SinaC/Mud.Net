using System;
using System.Linq;
using System.Text;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("look")]
        protected virtual bool DoLook(string rawParameters, params CommandParameter[] parameters)
        {
            // If no parameter or auto, display room + char in room + object in room
            if (String.IsNullOrWhiteSpace(rawParameters))
                DisplayRoom();
            else
                Send("NOT YET IMPLEMENTED");

            return true;
        }

        [Command("exits")]
        protected virtual bool DoExits(string rawParameters, params CommandParameter[] parameters)
        {
            DisplayExits(false);
            return true;
        }

        //
        private void DisplayRoom()
        {
            // Room name
            Send("%c%{0}%x%", Room.Name);
            // Room description
            Send(Room.Description);
            // Exits
            DisplayExits(true);
            foreach (IObject obj in Room.ObjectsInContainer) // TODO: compact mode (group by Blueprint)
                Send(obj.Name); // TODO: Objects in room  (see act_info.C:275 show_list_to_char)
            foreach (ICharacter character in Room.CharactersInRoom.Where(x => x != this))
                Send(character.Name); // TODO: Characters in room (see act_info.C:714 show_list_to_char)
        }

        private void DisplayExits(bool auto)
        {
            StringBuilder message = new StringBuilder();
            if (auto)
                message.Append("[Exits:");
            else
                message.AppendLine("Obvious exits:");
            bool exitFound = false;
            foreach (ServerOptions.ExitDirections direction in EnumHelpers.GetValues<ServerOptions.ExitDirections>())
            {
                IExit exit = Room.Exit(direction);
                if (exit != null && exit.Destination != null) // TODO: test if destination room is visible, if exit is visible, ...
                {
                    if (auto)
                    {
                        // TODO: 
                        // hidden+not bashed: [xxx]
                        // closed: (xxx)
                        // bashed: {{xxx}}
                        message.AppendFormat(" {0}", direction.ToString().ToLowerInvariant());
                    }
                    else
                    {
                        string destination = exit.Destination.Name; // TODO: 'room name' or 'too dark to tell' or 'closed door'
                        message.AppendLine(String.Format("{0} - {1} {2}{3}{4}",
                            StringHelpers.UpperFirstLetter(direction.ToString()),
                            destination,
                            String.Empty, // TODO: closed (DOOR)
                            String.Empty, // TODO: hidden [HIDDEN]
                            String.Empty)); // TODO: {{BASHED}}
                    }
                    exitFound = true;
                }
            }
            if (!exitFound)
            {
                if (auto)
                    message.Append(" none");
                else
                    message.AppendLine("None.");
            }
            if (auto)
                message.Append("]");
            Send(message.ToString());
        }
    }
}
