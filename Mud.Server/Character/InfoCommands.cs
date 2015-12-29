using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Logger;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        // 1/ if no parameter, look in room
        // 2/ else if 1st parameter is 'in' or 'on', search item (matching 2nd parameter) in the room, then inventory, then equipment, and display its content
        // 3/ else if a character can be found in room (matching 1st parameter), display character info
        // 4/ else if an item can be found in inventory+room (matching 1st parameter), display item description or extra description
        // 5/ else, if an extra description can be found in room (matching 1st parameter), display it
        // 6/ else, if 1st parameter is a direction, display if there is an exit/door
        [Command("look")]
        protected virtual bool DoLook(string rawParameters, params CommandParameter[] parameters)
        {
            // 1: room+exits+chars+items
            if (String.IsNullOrWhiteSpace(rawParameters))
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(1): room");
                DisplayRoom();
                return true;
            }
            // 2: container in room then inventory then equipment
            if (parameters[0].Value == "in" || parameters[0].Value == "on")
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(2): container in room, inventory, equipment");
                // look in container
                if (parameters.Length == 1)
                    Send("Look in what?");
                else
                {
                    // TODO: following code is stupid if room contains 2 identical items and inventory one, and we use look in item -> we'll never see item in inventory (look 3.item should display it <-- same case as look(4))
                    // search in room, then in inventory(unequiped), then in equipement
                    IItem obj = FindHelpers.FindByName(Room.Inside.Where(CanSee), parameters[1]) ?? FindHelpers.FindByName(Inside.Where(CanSee), parameters[1]); // TODO: filter on unequiped + equipment
                    if (obj == null)
                        Send(StringHelpers.ItemNotFound);
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
                return true;
            }
            // 3: character in room
            ICharacter character = FindHelpers.FindByName(Room.People.Where(CanSee), parameters[0]);
            if (character != null)
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(3): character in room");
                // TODO: peek ability check ???
                DisplayCharacter(character, true);
                return true;
            }
            // 4: search n'th item in inventory+room
            IItem item = FindHelpers.FindByName(Inside.Concat(Room.Inside), parameters[0]); // Concat preserves order!!!
            if (item != null)
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(4+5): item in {0}", item.ContainedInto);
                Send("{0}", item.Description);
                return true;
            }
            // 6: extra description in room  TODO
            // 7: direction
            ServerOptions.ExitDirections direction;
            if (EnumHelpers.TryFindByName(parameters[0].Value, out direction))
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(7): direction");
                IExit exit = Room.Exit(direction);
                if (exit == null || exit.Destination == null)
                    Send("Nothing special there.");
                else
                {
                    if (exit.Description != null)
                        Send(exit.Description);
                    else
                        Send("Nothing special there.");
                    // TODO: check if door + flags CLOSED/BASHED/HIDDEN
                }
            }
            else
                Send(StringHelpers.ItemNotFound);
            return true;
        }

        [Command("exits")]
        protected virtual bool DoExits(string rawParameters, params CommandParameter[] parameters)
        {
            DisplayExits(false);
            return true;
        }

        // Helpers
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

        private void DisplayCharacter(ICharacter character, bool peekInventory)
        {
            if (this == character)
                Act(ActOptions.ToRoom, "{0} looks at {0:m}self.", this);
            else
            {
                Act(ActOptions.ToVictim, character, "{0} looks at you.", this);
                Act(ActOptions.ToNotVictim, character, "{0} looks at {1}.", this, character);
            }
            Send("{0} is here.", character.Name);
            // TODO: health (instead of is here.), equipment  (see act_info.C:629 show_char_to_char_1)
            //Send("{0} is using:", character) if equipment not empty

            if (peekInventory)
            {
                Send("You peek at the inventory:");
                if (character.Inside.Count == 0)
                    Send("Nothing.");
                else
                    DisplayItems(character.Inside);
            }
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
