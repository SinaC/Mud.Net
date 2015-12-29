using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        // if no parameter, look in room
        // else if 1st parameter is 'in' or 'on', search item (matching 2nd parameter) in the room and display its content
        // else if a character can be found with 1st parameter, display character info
        // else search item in inventory and display info
        [Command("look")]
        protected virtual bool DoLook(string rawParameters, params CommandParameter[] parameters)
        {
            // If no parameter or auto, display room + chars in room + items in room
            if (String.IsNullOrWhiteSpace(rawParameters))
                DisplayRoom();
            else if (parameters[0].Value == "in" || parameters[0].Value == "on")
            {
                // look in container
                if (parameters.Length == 1)
                    Send("Look in what?");
                else
                {
                    // search in room for parameters[1]
                    IItem obj = FindHelpers.FindByName(Room.Inside.Where(CanSee), parameters[1]);
                    if (obj == null)
                        Send("You do not see that here.");
                    else
                    {
                        IContainer container = obj as IContainer;
                        if (container != null)
                        {
                            // TODO: check if closed
                            Send("{0} holds:", obj.Name);
                            if (container.Inside.Count == 0)
                                Send("Nothing");
                            else
                                DisplayItems(container.Inside);
                        }
                            // TODO: drink container
                        else
                            Send("This is not a container.");
                    }
                }
            }
            else
            {
                ICharacter character = FindHelpers.FindByName(Room.People.Where(CanSee), parameters[0]);
                if (character == null)
                    ; // TODO
            }

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
            DisplayItems(Room.Inside);
            foreach (ICharacter character in Room.People.Where(x => x != this))
                Send(character.Name); // TODO: Characters in room (see act_info.C:714 show_list_to_char)
        }

        private void DisplayItems(IEnumerable<IItem> items)
        {
            foreach (IItem item in items) // TODO: compact mode (group by Blueprint)
                Send(item.Name); // TODO: Items in room  (see act_info.C:275 show_list_to_char)
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
