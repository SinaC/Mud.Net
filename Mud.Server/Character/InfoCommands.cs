using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Logger;
using Mud.Server.Constants;
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
                    IItem containerItem = FindHelpers.FindItemByName(this, parameters[1]); // TODO: filter on unequiped + equipment
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
            //IItem item = FindHelpers.FindByName(Content.Concat(Room.Content), parameters[0]); // Concat preserves order!!!
            IItem item = FindHelpers.FindItemByName(this, parameters[0]);
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
                            where = "%C%<used as light>         %x%";
                            break;
                        case WearLocations.Head:
                            where = "%C%<worn on head>          %x%";
                            break;
                        case WearLocations.Amulet:
                            where = "%C%<worn on neck>          %x%";
                            break;
                        case WearLocations.Shoulders:
                            where = "%C%<worn around shoulders> %x%";
                            break;
                        case WearLocations.Chest:
                            where = "%C%<worn on chest>         %x%";
                            break;
                        case WearLocations.Cloak:
                            where = "%C%<worn about body>       %x%";
                            break;
                        case WearLocations.Waist:
                            where = "%C%<worn about waist>      %x%";
                            break;
                        case WearLocations.Wrists:
                            where = "%C%<worn around wrists>    %x%";
                            break;
                        case WearLocations.Hands:
                            where = "%C%<worn on hands>         %x%";
                            break;
                        case WearLocations.RingLeft:
                            where = "%C%<worn on left finger>   %x%";
                            break;
                        case WearLocations.RingRight:
                            where = "%C%<worn on right finger>   %x%";
                            break;
                        case WearLocations.Legs:
                            where = "%C%<worn on legs>          %x%";
                            break;
                        case WearLocations.Feet:
                            where = "%C%<worn on feet>          %x%";
                            break;
                        case WearLocations.Trinket1:
                            where = "%C%<worn as 1st trinket>   %x%";
                            break;
                        case WearLocations.Trinket2:
                            where = "%C%<worn as 2nd trinket>   %x%";
                            break;
                        case WearLocations.Wield:
                            where = "%C%<wielded>               %x%";
                            break;
                        case WearLocations.Wield2:
                            where = "%c%<offhand>               %x%";
                            break;
                        case WearLocations.Hold:
                            where = "%C%<held>                  %x%";
                            break;
                        case WearLocations.Shield:
                            where = "%C%<worn as shield>        %x%";
                            break;
                        default:
                            Log.Default.WriteLine(LogLevels.Error, "DoEquipment: missing WearLocation {0}", equipmentSlot.WearLocation);
                            break;
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

        [Command("examine")]
        protected virtual bool DoExamine(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Examine what or whom?" + Environment.NewLine);
            else
            {
                ICharacter character = FindHelpers.FindByName(Room.People, parameters[0]);
                if (character != null)
                {
                    Act(ActOptions.ToCharacter, "You examine {0}.", character);
                    Act(ActOptions.ToVictim, this, "{0} examines you.", character);
                    Act(ActOptions.ToNotVictim, this, "{0} examines {1}.", character);
                    DoLook(rawParameters, parameters); // TODO: call immediately sub-function
                    // TODO: display race and size
                }
                else
                {
                    IItem item = FindHelpers.FindItemByName(this, parameters[0]);
                    if (item != null)
                    {
                        Act(ActOptions.ToCharacter, "You examine {0}.", item);
                        Act(ActOptions.ToRoom, "{0} examines {1}.", this, item);
                        DoLook(rawParameters, parameters); // TODO: call immediately sub-function
                        IContainer container = item as IContainer;
                        if (container != null) // if container, display content
                        {
                            List<CommandParameter> newParameters = new List<CommandParameter>(parameters);
                            newParameters.Insert(0, new CommandParameter
                            {
                                Count = 1,
                                Value = "in"
                            });
                            DoLook("in " + rawParameters, newParameters.ToArray()); // TODO: call immediately sub-function
                        }
                    }
                    else
                        Send("You don't see any {0}." + Environment.NewLine, parameters[0]);
                }
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

        private void DisplayExits(bool compact)
        {
            StringBuilder message = new StringBuilder();
            if (compact)
                message.Append("[Exits:");
            else
                message.AppendLine("Obvious exits:");
            bool exitFound = false;
            foreach (ServerOptions.ExitDirections direction in EnumHelpers.GetValues<ServerOptions.ExitDirections>())
            {
                IExit exit = Room.Exit(direction);
                if (exit != null && exit.Destination != null) // TODO: test if destination room is visible, if exit is visible, ...
                {
                    if (compact)
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
                if (compact)
                    message.AppendLine(" none");
                else
                    message.AppendLine("None.");
            }
            if (compact)
                message.AppendLine("]");
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
