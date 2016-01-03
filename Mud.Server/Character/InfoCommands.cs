using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Logger;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Server;

namespace Mud.Server.Character
{
    public partial class Character
    {
        // 0/ if sleeping/blind/room is dark
        // 1/ else if no parameter, look in room
        // 2/ else if 1st parameter is 'in' or 'on', search item (matching 2nd parameter) in the room, then inventory, then equipment, and display its content
        // 3/ else if a character can be found in room (matching 1st parameter), display character info
        // 4/ else if an item can be found in inventory+room (matching 1st parameter), display item description or extra description
        // 5/ else, if an extra description can be found in room (matching 1st parameter), display it
        // 6/ else, if 1st parameter is a direction, display if there is an exit/door
        [Command("look")]
        protected virtual bool DoLook(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: 0/ sleeping/blind/dark room (see act_info.C:1413 -> 1436)

            // 1: room+exits+chars+items
            if (String.IsNullOrWhiteSpace(rawParameters))
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(1): room");
                DisplayRoom();
                return true;
            }
            // 2: container in room then inventory then equipment
            if (parameters[0].Value == "in" || parameters[0].Value == "on" || parameters[0].Value == "into")
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(2): container in room, inventory, equipment");
                // look in container
                if (parameters.Length == 1)
                    Send("Look in what?" + Environment.NewLine);
                else
                {
                    // search in room, then in inventory(unequiped), then in equipement
                    //// TODO: following code is stupid if room contains 2 identical items and inventory one, and we use look in item -> we'll never see item in inventory (look 3.item should display it <-- same case as look(4))
                    //IItem item = FindHelpers.FindByName(Room.Content.Where(CanSee), parameters[1]) ?? FindHelpers.FindByName(Content.Where(CanSee), parameters[1]); // TODO: filter on unequiped + equipment
                    IItem containerItem = FindHelpers.FindByName(Room.Content.Where(CanSee).Concat(Content.Where(CanSee)), parameters[1]); // TODO: filter on unequiped + equipment
                    if (containerItem == null)
                        Send(StringHelpers.ItemNotFound);
                    else
                    {
                        Log.Default.WriteLine(LogLevels.Debug, "DoLook(2): found in {0}", containerItem.ContainedInto.Name);
                        IContainer container = containerItem as IContainer;
                        if (container != null)
                        {
                            // TODO: check if closed
                            Send("{0} holds:" + Environment.NewLine, containerItem.DisplayName);
                            DisplayItems(container.Content, true, true);
                        }
                            // TODO: drink container
                        else
                            Send("This is not a container." + Environment.NewLine);
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
            IItem item = FindHelpers.FindByName(Content.Concat(Room.Content), parameters[0]); // Concat preserves order!!!
            if (item != null)
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(4+5): item in inventory+room -> {0}", item.ContainedInto.Name);
                Send("{0}" + Environment.NewLine, item.Description); // TODO: formatting
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
                    Send("Nothing special there." + Environment.NewLine);
                else
                {
                    if (exit.Description != null)
                        Send(exit.Description);
                    else
                        Send("Nothing special there." + Environment.NewLine);
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

        [Command("inventory")]
        protected virtual bool DoInventory(string rawParameters, params CommandParameter[] parameters)
        {
            Send("You are carrying:" + Environment.NewLine);
            DisplayItems(Content, true, true);
            return true;
        }

        [Command("equipment")]
        protected virtual bool DoEquipment(string rawParameters, params CommandParameter[] parameters)
        {
            Send("You are using:" + Environment.NewLine);
            if (Equipments.All(x => x.Item == null))
                Send("Nothing" + Environment.NewLine);
            else
                foreach (EquipmentSlot equipmentSlot in Equipments.Where(x => x.Item != null))
                {
                    string where = String.Empty;
                    switch (equipmentSlot.WearLocation)
                    {
                        case WearLocations.Light:
                            where = "%C%<used as light>      %x%";
                            break;
                        case WearLocations.Finger:
                            where = "%C%<worn on finger>     %x%";
                            break;
                        case WearLocations.Neck:
                            where = "%C%<worn on neck>       %x%";
                            break;
                        case WearLocations.Body:
                            where = "%C%<worn on torso>      %x%";
                            break;
                        case WearLocations.Head:
                            where = "%C%<worn on head>       %x%";
                            break;
                        case WearLocations.Legs:
                            where = "%C%<worn on legs>       %x%";
                            break;
                        case WearLocations.Feet:
                            where = "%C%<worn on feet>       %x%";
                            break;
                        case WearLocations.Hands:
                            where = "%C%<worn on hands>      %x%";
                            break;
                        case WearLocations.Arms:
                            where = "%C%<worn on arms>       %x%";
                            break;
                        case WearLocations.About:
                            where = "%C%<worn about body>    %x%";
                            break;
                        case WearLocations.Waist:
                            where = "%C%<worn about waist>   %x%";
                            break;
                        case WearLocations.Wrist:
                            where = "%C%<worn around wrist>  %x%";
                            break;
                        case WearLocations.Ear:
                            where = "%C%<worn on ear>        %x%";
                            break;
                        case WearLocations.Eyes:
                            where = "%C%<worn on eyes>       %x%";
                            break;
                        case WearLocations.Float:
                            where = "%C%<floating nearby>    %x%";
                            break;
                            //"{C<worn as shield>        {x",
                            //"{c<wielded>		{x",
                            //"{C<held>			{x",
                            //"{c<secondary weapon>	{x",
                            //"{C<floating nearby>	{x",
                            //  "{B<brand mark>            {x"
                            //"{c<third weapon>          {x",
                            //"{c<fourth weapon>         {x",
                    }
                    StringBuilder sb = new StringBuilder(where);
                    if (CanSee(equipmentSlot.Item))
                        sb.AppendLine(FormatItem(equipmentSlot.Item, true).ToString());
                    else
                        sb.AppendLine("something.");
                    Send(sb);
                }
            return true;
        }

        [Command("scan")]
        protected virtual bool DoScan(string rawParameters, params CommandParameter[] parameters)
        {
            // Current room
            Send("Right here you see:" + Environment.NewLine);
            StringBuilder currentScan = ScanRoom(Room);
            if (currentScan.Length == 0)
                Send("Noone" + Environment.NewLine); // should never happen, 'this' is in the room
            else
                Send(currentScan); // no need to add CRLF
            // Scan in one direction for each distance, then starts with another direction
            foreach (ServerOptions.ExitDirections direction in EnumHelpers.GetValues<ServerOptions.ExitDirections>())
            {
                IRoom currentRoom = Room; // starting point
                for (int distance = 1; distance < 4; distance++)
                {
                    IRoom destination = currentRoom.GetRoom(direction);
                    if (destination == null)
                        break; // stop in that direction if no exit found
                    StringBuilder roomScan = ScanRoom(destination);
                    if (roomScan.Length > 0)
                    {
                        Send("%c%{0} %r%{1}%x% from here you see:" + Environment.NewLine, distance, direction);
                        Send(roomScan); // no need to add CRLF
                        currentRoom = destination;
                    }
                }
            }
            // TODO: there is one too many CRLF
            return true;
        }

        //********************************************************************
        // Helpers
        //********************************************************************
        private void DisplayRoom() // equivalent to act_info.C:do_look("auto")
        {
            // Room name
            Send("%c%{0}%x%" + Environment.NewLine, Room.DisplayName);
            // Room description
            Send(Room.Description);
            // Exits
            DisplayExits(true);
            DisplayItems(Room.Content, false, false);
            foreach (ICharacter character in Room.People.Where(x => x != this))
            { //  (see act_info.C:714 show_char_to_char)
                if (CanSee(character)) // see act_info.C:375 show_char_to_char_0)
                {
                    // TODO: display flags (see act_info.C:387 -> 478)
                    // TODO: display long description and stop
                    // TODO: display position (see act_info.C:505 -> 612)
                    Send("{0} is here." + Environment.NewLine, character.DisplayName); // last case of POS_STANDING
                }
                else
                    ; // TODO: INFRARED (see act_info.C:728)
            }
        }

        private void DisplayCharacter(ICharacter character, bool peekInventory) // equivalent to act_info.C:show_char_to_char_1
        {
            if (this == character)
                Act(ActOptions.ToRoom, "{0} looks at {0:m}self.", this);
            else
            {
                Act(ActOptions.ToVictim, character, "{0} looks at you.", this);
                Act(ActOptions.ToNotVictim, character, "{0} looks at {1}.", this, character);
            }
            Send("{0} is here." + Environment.NewLine, character.DisplayName);
            // TODO: health (instead of is here.), equipments  (see act_info.C:629 show_char_to_char_1)
            //Send("{0} is using:", character) if equipment not empty

            if (peekInventory)
            {
                Send("You peek at the inventory:" + Environment.NewLine);
                DisplayItems(character.Content, true, true);
            }
        }

        private void DisplayItems(IEnumerable<IItem> items, bool shortDisplay, bool displayNothing) // equivalent to act_info.C:show_list_to_char
        {
            IEnumerable<IItem> enumerable = items as IItem[] ?? items.ToArray();
            if (displayNothing && !enumerable.Any())
                Send("Nothing." + Environment.NewLine);
            else
            {
                foreach (IItem item in enumerable) // TODO: compact mode (grouped by Blueprint)
                    Send(FormatItem(item, shortDisplay) + Environment.NewLine); // TODO: (see act_info.C:170 format_obj_to_char)
            }
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
                        string destination = exit.Destination.DisplayName; // TODO: 'room name' or 'too dark to tell' or 'closed door'
                        message.AppendFormatLine("{0} - {1} {2}{3}{4}",
                            StringHelpers.UpperFirstLetter(direction.ToString()),
                            destination,
                            String.Empty, // TODO: closed (DOOR)
                            String.Empty, // TODO: hidden [HIDDEN]
                            String.Empty); // TODO: {{BASHED}}
                    }
                    exitFound = true;
                }
            }
            if (!exitFound)
            {
                if (auto)
                    message.Append(" none");
                else
                    message.Append("None.");
            }
            if (auto)
                message.Append("]");
            message.AppendLine();
            Send(message);
        }

        private StringBuilder ScanRoom(IRoom room)
        {
            StringBuilder peopleInRoom = new StringBuilder();
            foreach (ICharacter character in room.People.Where(CanSee))
                peopleInRoom.AppendFormatLine(" - {0}", character.DisplayName);
            return peopleInRoom;
        }

        private StringBuilder FormatItem(IItem item, bool shortDisplay)
        {
            StringBuilder sb = new StringBuilder();
            // TODO: affects
            sb.Append(shortDisplay
                ? item.DisplayName
                : item.Description);
            return sb;
        }
    }
}
