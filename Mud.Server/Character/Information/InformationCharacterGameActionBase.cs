using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Logger;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Character.Information
{
    public abstract class InformationCharacterGameActionBase : CharacterGameAction
    {
        protected StringBuilder AppendRoom(StringBuilder sb, IRoom room) // equivalent to act_info.C:do_look("auto")
        {
            IPlayableCharacter playableCharacter = Actor as IPlayableCharacter;
            // Room name
            if (playableCharacter?.IsImmortal == true)
                sb.AppendFormatLine($"%c%{room.DisplayName} [{room.Blueprint?.Id.ToString() ?? "???"}]%x%");
            else
                sb.AppendFormatLine("%c%{0}%x%", room.DisplayName);
            // Room description
            sb.Append(room.Description);
            // Exits
            if (playableCharacter != null && playableCharacter.AutoFlags.HasFlag(AutoFlags.Exit))
                AppendExits(sb, Actor.Room, true);
            AppendItems(sb, room.Content.Where(x => Actor.CanSee(x)), false, false);
            AppendCharacters(sb, room);
            return sb;
        }

        protected StringBuilder AppendCharacters(StringBuilder sb, IRoom room)
        {
            foreach (ICharacter victim in room.People.Where(x => x != Actor))
            {
                //  (see act_info.C:714 show_char_to_char)
                if (Actor.CanSee(victim)) // see act_info.C:375 show_char_to_char_0)
                    AppendCharacterInRoom(sb, victim);
                else if (room.IsDark && victim.CharacterFlags.HasFlag(CharacterFlags.Infrared))
                    sb.AppendLine("You see glowing red eyes watching YOU!");
            }

            return sb;
        }

        protected StringBuilder AppendContainerContent(StringBuilder sb, IContainer container)
        {
            sb.AppendFormatLine("{0} holds:", container.RelativeDisplayName(Actor));
            AppendItems(sb, container.Content, true, true);
            return sb;
        }

        protected StringBuilder AppendCharacterInRoom(StringBuilder sb, ICharacter victim)
        {
            // display flags
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Charm))
                sb.Append("%C%(Charmed)%x%");
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Flying))
                sb.Append("%c%(Flying)%x%");
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Invisible))
                sb.Append("%y%(Invis)%x%");
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Hide))
                sb.Append("%b%(Hide)%x%");
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Sneak))
                sb.Append("%R%(Sneaking)%x%");
            if (victim.CharacterFlags.HasFlag(CharacterFlags.PassDoor))
                sb.Append("%c%(Translucent)%x%");
            if (victim.CharacterFlags.HasFlag(CharacterFlags.FaerieFire))
                sb.Append("%m%(Pink Aura)%x%");
            if (victim.CharacterFlags.HasFlag(CharacterFlags.DetectEvil))
                sb.Append("%r%(Red Aura)%x%");
            if (victim.CharacterFlags.HasFlag(CharacterFlags.DetectGood))
                sb.Append("%Y%(Golden Aura)%x%");
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Sanctuary))
                sb.Append("%W%(White Aura)%x%");
            // TODO: killer/thief
            // TODO: display long description and stop if position = start position for NPC

            // last case of POS_STANDING
            sb.Append(victim.RelativeDisplayName(Actor));
            switch (victim.Position)
            {
                case Positions.Stunned:
                    sb.Append(" is lying here stunned.");
                    break;
                case Positions.Sleeping:
                    AppendPositionFurniture(sb, "sleeping", victim.Furniture);
                    break;
                case Positions.Resting:
                    AppendPositionFurniture(sb, "resting", victim.Furniture);
                    break;
                case Positions.Sitting:
                    AppendPositionFurniture(sb, "sitting", victim.Furniture);
                    break;
                case Positions.Standing:
                    if (Actor.Furniture != null)
                        AppendPositionFurniture(sb, "standing", victim.Furniture);
                    else
                        sb.Append(" is here");
                    break;
                case Positions.Fighting:
                    sb.Append(" is here, fighting ");
                    if (victim.Fighting == null)
                    {
                        Log.Default.WriteLine(LogLevels.Warning, "{0} position is fighting but fighting is null.", victim.DebugName);
                        sb.Append("thing air??");
                    }
                    else if (victim.Fighting == Actor)
                        sb.Append("YOU!");
                    else if (victim.Room == victim.Fighting.Room)
                        sb.AppendFormat("{0}.", victim.Fighting.RelativeDisplayName(Actor));
                    else
                    {
                        Log.Default.WriteLine(LogLevels.Warning, "{0} is fighting {1} in a different room.", victim.DebugName, victim.Fighting.DebugName);
                        sb.Append("someone who left??");
                    }
                    break;
            }
            sb.AppendLine();
            return sb;
        }

        protected StringBuilder AppendPositionFurniture(StringBuilder sb, string verb, IItemFurniture furniture)
        {
            if (furniture == null)
                sb.AppendFormat(" is {0} here.", verb);
            else
            {
                if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                    sb.AppendFormat(" is {0} at {1}", verb, furniture.DisplayName);
                else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                    sb.AppendFormat(" is {0} on {1}", verb, furniture.DisplayName);
                else if (furniture.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                    sb.AppendFormat(" is {0} in {1}", verb, furniture.DisplayName);
                else
                    sb.AppendFormat(" is {0} here.", verb);
            }
            return sb;
        }

        protected StringBuilder AppendCharacter(StringBuilder sb, ICharacter victim, bool peekInventory) // equivalent to act_info.C:show_char_to_char_1
        {
            //
            string condition = "is here.";
            int maxHitPoints = victim.MaxHitPoints;
            if (maxHitPoints > 0)
            {
                int percent = (100 * victim.HitPoints) / maxHitPoints;
                if (percent >= 100)
                    condition = "is in excellent condition.";
                else if (percent >= 90)
                    condition = "has a few scratches.";
                else if (percent >= 75)
                    condition = "has some small wounds and bruises.";
                else if (percent >= 50)
                    condition = "has quite a few wounds.";
                else if (percent >= 30)
                    condition = "has some big nasty wounds and scratches.";
                else if (percent >= 15)
                    condition = "looks pretty hurt.";
                else if (percent >= 0)
                    condition = "is in awful condition.";
                else
                    condition = "is bleeding to death.";
            }
            sb.AppendLine($"{victim.RelativeDisplayName(Actor)} {condition}");

            //
            if (victim.Equipments.Any(x => x.Item != null))
            {
                sb.AppendLine($"{victim.RelativeDisplayName(Actor)} is using:");
                foreach (EquippedItem equippedItem in victim.Equipments.Where(x => x.Item != null))
                {
                    sb.Append(EquipmentSlotsToString(equippedItem));
                    equippedItem.Item.Append(sb, Actor, true);
                    sb.AppendLine();
                }
            }

            if (peekInventory)
            {
                sb.AppendLine("You peek at the inventory:");
                IEnumerable<IItem> items = Actor == victim
                    ? victim.Inventory
                    : victim.Inventory.Where(x => Actor.CanSee(x)); // don't display 'invisible item' when inspecting someone else
                AppendItems(sb, items, true, true);
            }

            return sb;
        }

        protected StringBuilder AppendItems(StringBuilder sb, IEnumerable<IItem> items, bool shortDisplay, bool displayNothing) // equivalent to act_info.C:show_list_to_char
        {
            var enumerable = items as IItem[] ?? items.ToArray();
            if (displayNothing && !enumerable.Any())
                sb.AppendLine("Nothing.");
            else
            {
                // Grouped by description
                foreach (var groupedFormattedItem in enumerable.Select(item => item.Append(new StringBuilder(), Actor, shortDisplay).ToString()).GroupBy(x => x))
                {
                    int count = groupedFormattedItem.Count();
                    if (count > 1)
                        sb.AppendFormatLine("%W%({0,2})%x% {1}", count, groupedFormattedItem.Key);
                    else
                        sb.AppendFormatLine("     {0}", groupedFormattedItem.Key);
                }
            }

            return sb;
        }

        protected StringBuilder AppendExits(StringBuilder sb, IRoom room, bool compact)
        {
            if (compact)
                sb.Append("[Exits:");
            else if (Actor is IPlayableCharacter playableCharacter && playableCharacter.IsImmortal)
                sb.AppendFormatLine($"Obvious exits from room {room.Blueprint?.Id.ToString() ?? "???"}:");
            else
                sb.AppendLine("Obvious exits:");
            bool exitFound = false;
            foreach (ExitDirections direction in EnumHelpers.GetValues<ExitDirections>())
            {
                IExit exit = room[direction];
                IRoom destination = exit?.Destination;
                if (destination != null && Actor.CanSee(exit))
                {
                    if (compact)
                    {
                        sb.Append(" ");
                        if (exit.IsHidden)
                            sb.Append("[");
                        if (exit.IsClosed)
                            sb.Append("(");
                        sb.AppendFormat("{0}", direction.ToString().ToLowerInvariant());
                        if (exit.IsClosed)
                            sb.Append(")");
                        if (exit.IsHidden)
                            sb.Append("]");
                    }
                    else
                    {
                        sb.Append(direction.DisplayName());
                        sb.Append(" - ");
                        if (exit.IsClosed)
                            sb.Append("A closed door");
                        else if (destination.IsDark)
                            sb.Append("Too dark to tell");
                        else
                            sb.Append(exit.Destination.DisplayName);
                        if (exit.IsClosed)
                            sb.Append(" (CLOSED)");
                        if (exit.IsHidden)
                            sb.Append(" [HIDDEN]");
                        if (Actor is IPlayableCharacter playableCharacter && playableCharacter.IsImmortal)
                            sb.Append($" (room {exit.Destination.Blueprint?.Id.ToString() ?? "???"})");
                        sb.AppendLine();
                    }
                    exitFound = true;
                }
            }
            if (!exitFound)
            {
                if (compact)
                    sb.AppendLine(" none");
                else
                    sb.AppendLine("None.");
            }
            if (compact)
                sb.AppendLine("]");
            return sb;
        }

        protected bool FindItemByExtraDescriptionOrName(ICommandParameter parameter, out string description) // Find by extra description then name (search in inventory, then equipment, then in room)
        {
            description = null;
            int count = 0;
            foreach (IItem item in Actor.Inventory.Where(x => Actor.CanSee(x))
                .Concat(Actor.Equipments.Where(x => x.Item != null && Actor.CanSee(x.Item)).Select(x => x.Item))
                .Concat(Actor.Room.Content.Where(x => Actor.CanSee(x))))
            {
                // Search in item extra description keywords
                if (item.ExtraDescriptions != null)
                {
                    foreach (var extraDescription in item.ExtraDescriptions)
                        if (parameter.Tokens.All(x => StringCompareHelpers.StringStartsWith(extraDescription.Key, x))
                            && ++count == parameter.Count)
                        {
                            description = extraDescription.FirstOrDefault(); // TODO: really what we want ?
                            return true;
                        }
                }
                // Search in item keywords
                if (StringCompareHelpers.StringListsStartsWith(item.Keywords, parameter.Tokens)
                    && ++count == parameter.Count)
                {
                    StringBuilder sb = new StringBuilder();
                    description = item.Append(sb, Actor, false).ToString();
                    return true;
                }
            }
            return false;
        }

        protected string EquipmentSlotsToString(EquippedItem equippedItem)
        {
            switch (equippedItem.Slot)
            {
                case EquipmentSlots.Light:
                    return "%C%<used as light>          %x%";
                case EquipmentSlots.Head:
                    return "%C%<worn on head>           %x%";
                case EquipmentSlots.Amulet:
                    return "%C%<worn on neck>           %x%";
                case EquipmentSlots.Chest:
                    return "%C%<worn on chest>          %x%";
                case EquipmentSlots.Cloak:
                    return "%C%<worn about body>        %x%";
                case EquipmentSlots.Waist:
                    return "%C%<worn about waist>       %x%";
                case EquipmentSlots.Wrists:
                    return "%C%<worn around wrists>     %x%";
                case EquipmentSlots.Arms:
                    return "%C%<worn on arms>           %x%";
                case EquipmentSlots.Hands:
                    return "%C%<worn on hands>          %x%";
                case EquipmentSlots.Ring:
                    return "%C%<worn on finger>         %x%";
                case EquipmentSlots.Legs:
                    return "%C%<worn on legs>           %x%";
                case EquipmentSlots.Feet:
                    return "%C%<worn on feet>           %x%";
                case EquipmentSlots.MainHand:
                    return "%C%<wielded>                %x%";
                case EquipmentSlots.OffHand:
                    if (equippedItem.Item != null)
                    {
                        if (equippedItem.Item is IItemShield)
                            return "%C%<worn as shield>         %x%";
                        if (equippedItem.Item.WearLocation == WearLocations.Hold)
                            return "%C%<held>                   %x%";
                    }
                    return "%c%<offhand>                %x%";
                case EquipmentSlots.Float:
                    return "%C%<floating nearby>        %x%";
                default:
                    Log.Default.WriteLine( LogLevels.Error, "DoEquipment: missing WearLocation {0}", equippedItem.Slot);
                    break;
            }
            return "%C%<unknown>%x%";
        }
    }
}
