using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character
{
    public partial class CharacterBase
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
        protected virtual CommandExecutionResults DoLook(string rawParameters, params CommandParameter[] parameters)
        {
            // 0: sleeping/blind/dark room
            if (Position < Positions.Sleeping)
            {
                Send("You can't see anything but stars!");
                return CommandExecutionResults.NoExecution;
            }
            if (Position == Positions.Sleeping)
            {
                Send("You can't see anything, you're sleeping!");
                return CommandExecutionResults.NoExecution;
            }
            if (CharacterFlags.HasFlag(CharacterFlags.Blind))
            {
                Send("You can't see a thing!");
                return CommandExecutionResults.NoExecution;
            }
            if (Room.IsDark)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("It is pitch black ... ");
                AppendCharacters(sb, Room);
                Send(sb);
                return CommandExecutionResults.Ok;
            }

            // 1: room+exits+chars+items
            if (string.IsNullOrWhiteSpace(rawParameters))
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(1): room");
                StringBuilder sb = new StringBuilder();
                AppendRoom(sb, Room);
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            // 2: container in room then inventory then equipment
            if (parameters[0].Value == "in" || parameters[0].Value == "on" || parameters[0].Value == "into")
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(2): container in room, inventory, equipment");
                // look in container
                if (parameters.Length == 1)
                {
                    Send("Look in what?");
                    return CommandExecutionResults.SyntaxErrorNoDisplay;
                }
                // search in room, then in inventory(unequipped), then in equipment
                IItem containerItem = FindHelpers.FindItemHere(this, parameters[1]);
                if (containerItem == null)
                {
                    Send(StringHelpers.ItemNotFound);
                    return CommandExecutionResults.TargetNotFound;
                }

                Log.Default.WriteLine(LogLevels.Debug, "DoLook(2): found in {0}", containerItem.ContainedInto.DebugName);

                // drink container
                if (containerItem is IItemDrinkContainer itemDrinkContainer)
                {
                    if (itemDrinkContainer.IsEmpty)
                        Send("It's empty.");
                    else
                    {
                        string left = itemDrinkContainer.LiquidLeft < itemDrinkContainer.MaxLiquid / 4
                            ? "less than half-"
                            : (itemDrinkContainer.LiquidLeft < (3 * itemDrinkContainer.MaxLiquid) / 4
                                ? "about half-"
                                : "more than half-");
                        var liquidInfo = TableValues.LiquidInfo(itemDrinkContainer.LiquidName);
                        Send("It's {0}filled with a {1} liquid.", left, liquidInfo.color);
                    }
                    return CommandExecutionResults.Ok;
                }
                // other container
                IContainer container = containerItem as IContainer;
                if (container == null)
                {
                    Send("This is not a container.");
                    return CommandExecutionResults.InvalidTarget;
                }
                // closed ?
                if (containerItem is ICloseable closeable && closeable.IsClosed)
                {
                    Send("It's closed.");
                    return CommandExecutionResults.Ok;
                }
                StringBuilder sb = new StringBuilder();
                AppendContainerContent(sb, container);
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            // 3: character in room
            ICharacter victim = FindHelpers.FindByName(Room.People.Where(CanSee), parameters[0]);
            if (victim != null)
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(3): character in room");
                if (this == victim)
                    Act(ActOptions.ToRoom, "{0} looks at {0:m}self.", this);
                else
                    Act(ActOptions.ToRoom, "{0} looks at {1}.", this, victim);
                StringBuilder sb = new StringBuilder();
                AppendCharacter(sb, victim, true); // TODO: always peeking ???
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            // 4: search among inventory/equipment/room.content if an item has extra description or name equals to parameters
            string itemDescription;
            bool itemFound = FindItemByExtraDescriptionOrName(parameters[0], out itemDescription);
            if (itemFound)
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(4): item in inventory+equipment+room -> {0}", itemDescription);
                Send(itemDescription, false);
                return CommandExecutionResults.Ok;
            }
            // 5: extra description in room
            if (Room.ExtraDescriptions != null && Room.ExtraDescriptions.Any())
            {
                // TODO: try to use ElementAtOrDefault
                int count = 0;
                foreach (var extraDescription in Room.ExtraDescriptions)
                {
                    if (parameters[0].Tokens.All(x => StringCompareHelpers.StringStartsWith(extraDescription.Key, x))
                            && ++count == parameters[0].Count)
                    {
                        foreach(string desc in extraDescription)
                            Send(desc, false);
                        return CommandExecutionResults.Ok;
                    }
                }
            }
            // 6: direction
            ExitDirections direction;
            if (ExitDirectionsExtensions.TryFindDirection(parameters[0].Value, out direction))
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(6): direction");
                IExit exit = Room[direction];
                if (exit?.Destination == null)
                    Send("Nothing special there.");
                else
                {
                    Send(exit.Description ?? "Nothing special there.");
                    if (exit.Keywords.Any())
                    {
                        string exitName = exit.Keywords.FirstOrDefault() ?? "door";
                        if (exit.IsClosed)
                            Send("The {0} is closed.", exitName);
                        else if (exit.IsDoor)
                            Send("The {0} is open.", exitName);
                    }
                }
                return CommandExecutionResults.Ok;
            }
            //
            Send(StringHelpers.ItemNotFound);
            return CommandExecutionResults.TargetNotFound;
        }

        [CharacterCommand("exits", "Information", MinPosition = Positions.Resting)]
        protected virtual CommandExecutionResults DoExits(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            AppendExits(sb, false);
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("examine", "Information", MinPosition = Positions.Resting)]
        [Syntax(
            "[cmd] item",
            "[cmd] <container>",
            "[cmd] <corpse>")]
        protected virtual CommandExecutionResults DoExamine(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Examine what or whom?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            // character
            ICharacter victim = FindHelpers.FindByName(Room.People, parameters[0]);
            if (victim != null)
            {
                Act(ActOptions.ToAll, "{0:N} examine{0:v} {1}.", this, victim);
                StringBuilder sbCharacter = new StringBuilder();
                AppendCharacter(sbCharacter, victim, true);
                // TODO: display race and size
                Send(sbCharacter);
                return CommandExecutionResults.Ok;
            }
            // item
            IItem item = FindHelpers.FindItemHere(this, parameters[0]);
            if (item == null)
            {
                Send("You don't see any {0}.", parameters[0]);
                return CommandExecutionResults.TargetNotFound;
            }
            //
            Act(ActOptions.ToAll, "{0:N} examine{0:v} {1}.", this, item);
            StringBuilder sbItem = new StringBuilder();
                switch (item)
                {
                    case IContainer container:
                        if (container is IItemContainer itemContainer && itemContainer.IsClosed)
                            sbItem.AppendLine("It's closed.");
                        else
                            AppendContainerContent(sbItem, container);
                        break;
                    case IItemMoney money:
                        if (money.SilverCoins == 0 && money.GoldCoins == 0)
                            sbItem.AppendLine("Odd...there's no coins in the pile.");
                        else if (money.SilverCoins == 0 && money.GoldCoins > 0)
                        {
                            if (money.GoldCoins == 1)
                                sbItem.AppendLine("Wow. one gold coin.");
                            else
                                sbItem.AppendFormatLine("There are {0} gold coins in the pile.", money.GoldCoins);
                        }
                        else if (money.SilverCoins > 0 && money.GoldCoins == 0)
                        {
                            if (money.SilverCoins == 1)
                                sbItem.AppendLine("Wow. one silver coin.");
                            else
                                sbItem.AppendFormatLine("There are {0} silver coins in the pile.", money.SilverCoins);
                        }
                        else
                            sbItem.AppendFormatLine("There are {0} gold and {1} silver coins in the pile.", money.SilverCoins, money.GoldCoins);
                        break;
                    default:
                        sbItem.AppendLine(FormatItem(item, true));
                        break;
                }

            Send(sbItem);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("scan", "Information", MinPosition = Positions.Standing)]
        protected virtual CommandExecutionResults DoScan(string rawParameters, params CommandParameter[] parameters)
        {
            if (Room == null)
                return CommandExecutionResults.Error;
            if (Room.RoomFlags.HasFlag(RoomFlags.NoScan))
            {
                Send("Your vision is clouded by a mysterious force.");
                return CommandExecutionResults.InvalidTarget;
            }

            StringBuilder sb = new StringBuilder(1024);
            // Current room
            sb.AppendLine("Right here you see:");
            StringBuilder currentScan = ScanRoom(Room);
            if (currentScan.Length == 0)
                sb.AppendLine("None");// should never happen, 'this' is in the room
            else
                sb.Append(currentScan);
            // Scan in one direction for each distance, then starts with another direction
            foreach (ExitDirections direction in EnumHelpers.GetValues<ExitDirections>())
            {
                IRoom currentRoom = Room; // starting point
                for (int distance = 1; distance < 4; distance++)
                {
                    IExit exit = currentRoom[direction];
                    if (exit == null)
                        break; // stop in that direction if no exit found
                    IRoom destination = exit?.Destination;
                    if (destination == null)
                        break; // stop in that direction if exit without linked room found
                    if (destination.RoomFlags.HasFlag(RoomFlags.NoScan))
                        break; // no need to scan further
                    if (exit.IsClosed)
                        break; // can't see thru closed door
                    StringBuilder roomScan = ScanRoom(destination);
                    if (roomScan.Length > 0)
                    {
                        sb.AppendFormatLine("%c%{0} %r%{1}%x% from here you see:", distance, direction);
                        sb.Append(roomScan);
                    }

                    currentRoom = destination;
                }
            }
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("affects", "Information")]
        [CharacterCommand("auras", "Information")]
        protected virtual CommandExecutionResults DoAffects(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            if (Auras.Any() || PeriodicAuras.Any())
            {
                sb.AppendLine("%c%You are affected by the following auras:%x%");
                // Auras
                foreach (IAura aura in Auras.Where(x => !x.AuraFlags.HasFlag(AuraFlags.Hidden)).OrderBy(x => x.PulseLeft))
                    aura.Append(sb);
                // TODO
                //// Periodic auras
                //foreach (IPeriodicAura pa in _periodicAuras.Where(x => x.Ability == null || (x.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden))
                //{
                //    if (pa.AuraType == PeriodicAuraTypes.Damage)
                //        sb.AppendFormatLine("%B%{0}%x% %W%deals {1}{2}%x% {3} damage every %g%{4}%x% for %c%{5}%x%",
                //            pa.Ability == null ? "Unknown" : pa.Ability.Name,
                //            pa.Amount,
                //            pa.AmountOperator == AmountOperators.Fixed ? string.Empty : "%",
                //            StringHelpers.SchoolTypeColor(pa.School),
                //            StringHelpers.FormatDelay(pa.TickDelay),
                //            StringHelpers.FormatDelay(pa.SecondsLeft));
                //    else
                //        sb.AppendFormatLine("%B%{0}%x% %W%heals {1}{2}%x% hp every %g%{3}%x% for %c%{4}%x%",
                //            pa.Ability == null ? "Unknown" : pa.Ability.Name,
                //            pa.Amount,
                //            pa.AmountOperator == AmountOperators.Fixed ? string.Empty : "%",
                //            StringHelpers.FormatDelay(pa.TickDelay),
                //            StringHelpers.FormatDelay(pa.SecondsLeft));
                //}
            }
            else
                sb.AppendLine("%c%You are not affected by any spells.%x%");
            Page(sb);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("saffects", "Information")]
        [CharacterCommand("sauras", "Information")]
        protected virtual CommandExecutionResults DoShortAffects(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            if (Auras.Any() || PeriodicAuras.Any())
            {
                sb.AppendLine("%c%You are affected by the following auras:%x%");
                // Auras
                foreach (IAura aura in Auras.Where(x => !x.AuraFlags.HasFlag(AuraFlags.Hidden)).OrderBy(x => x.PulseLeft))
                    aura.Append(sb, true);
                // TODO
                //// Periodic auras
                //foreach (IPeriodicAura pa in _periodicAuras.Where(x => x.Ability == null || (x.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden))
                //{
                //    if (pa.AuraType == PeriodicAuraTypes.Damage)
                //        sb.AppendFormatLine("%B%{0}%x% %W%deals {1}{2}%x% {3} damage every %g%{4}%x% for %c%{5}%x%",
                //            pa.Ability == null ? "Unknown" : pa.Ability.Name,
                //            pa.Amount,
                //            pa.AmountOperator == AmountOperators.Fixed ? string.Empty : "%",
                //            StringHelpers.SchoolTypeColor(pa.School),
                //            StringHelpers.FormatDelay(pa.TickDelay),
                //            StringHelpers.FormatDelay(pa.SecondsLeft));
                //    else
                //        sb.AppendFormatLine("%B%{0}%x% %W%heals {1}{2}%x% hp every %g%{3}%x% for %c%{4}%x%",
                //            pa.Ability == null ? "Unknown" : pa.Ability.Name,
                //            pa.Amount,
                //            pa.AmountOperator == AmountOperators.Fixed ? string.Empty : "%",
                //            StringHelpers.FormatDelay(pa.TickDelay),
                //            StringHelpers.FormatDelay(pa.SecondsLeft));
                //}
            }
            else
                sb.AppendLine("%c%You are not affected by any spells.%x%");
            Page(sb);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("score", "Information", Priority = 2)]
        protected virtual CommandExecutionResults DoScore(string rawParameters, params CommandParameter[] parameters)
        {
            IPlayableCharacter pc = this as IPlayableCharacter;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("+--------------------------------------------------------+"); // length 1 + 56 + 1
            sb.AppendLine("|" + DisplayName.CenterText(56) + "|");
            sb.AppendLine("+------------------------------+-------------------------+");
            sb.AppendLine("| %W%Attributes%x%                   |                         |");
            sb.AppendFormatLine("| %c%Strength     : %W%[{0,5}/{1,5}]%x% | %c%Race   : %W%{2,14}%x% |", this[CharacterAttributes.Strength], BaseAttribute(CharacterAttributes.Strength), Race?.DisplayName ?? "(none)");
            sb.AppendFormatLine("| %c%Intelligence : %W%[{0,5}/{1,5}]%x% | %c%Class  : %W%{2,14}%x% |", this[CharacterAttributes.Intelligence], BaseAttribute(CharacterAttributes.Intelligence), Class?.DisplayName ?? "(none)");
            sb.AppendFormatLine("| %c%Wisdom       : %W%[{0,5}/{1,5}]%x% | %c%Sex    : %W%{2,14}%x% |", this[CharacterAttributes.Wisdom], BaseAttribute(CharacterAttributes.Wisdom), Sex);
            sb.AppendFormatLine("| %c%Dexterity    : %W%[{0,5}/{1,5}]%x% | %c%Level  : %W%{2,14}%x% |", this[CharacterAttributes.Dexterity], BaseAttribute(CharacterAttributes.Dexterity), Level);
            if (pc != null)
                sb.AppendFormatLine("| %c%Constitution : %W%[{0,5}/{1,5}]%x% | %c%NxtLvl : %W%{2,14}%x% |", this[CharacterAttributes.Constitution], BaseAttribute(CharacterAttributes.Constitution), pc.ExperienceToLevel);
            else
                sb.AppendFormatLine("| %c%Constitution : %W%[{0,5}/{1,5}]%x% |                       |", this[CharacterAttributes.Constitution], BaseAttribute(CharacterAttributes.Constitution), Level);
            sb.AppendLine("+------------------------------+-------------------------+");
            sb.AppendLine("| %W%Resources%x%                    | %W%Defensive%x%              |");
            sb.AppendFormatLine("| %g%Hp     : %W%[{0,8}/{1,8}]%x% | %g%Bash         : %W%[{2,6}]%x% |", HitPoints, MaxHitPoints, this[Armors.Bash]);
            sb.AppendFormatLine("| %g%Move   : %W%[{0,8}/{1,8}]%x% | %g%Pierce       : %W%[{2,6}]%x% |", MovePoints, this.MaxMovePoints, this[Armors.Pierce]);
            List<string> resources = new List<string>();
            foreach (ResourceKinds resourceKind in CurrentResourceKinds)
                resources.Add($"%g%{resourceKind,-7}: %W%[{this[resourceKind],8}/{MaxResource(resourceKind),8}]%x%");
            if (resources.Count < 3)
                resources.AddRange(Enumerable.Repeat("                            ", 3 - resources.Count));
            sb.AppendFormatLine("| {0} | %g%Slash        : %W%[{1,6}]%x% |", resources[0], this[Armors.Slash]);
            sb.AppendFormatLine("| {0} | %g%Exotic       : %W%[{1,6}]%x% |", resources[1], this[Armors.Exotic]);
            sb.AppendFormatLine("| {0} | %g%Saves        : %W%[{1,6}]%x% |", resources[2], this[CharacterAttributes.SavingThrow]);
            sb.AppendLine("+------------------------------+-------------------------+");
            if (pc != null)
                sb.AppendFormatLine("| %g%Hit:  %W%{0,6}%x%    %g%Dam:  %W%{1,6}%x% | %g%Train: %W%{2,3}%x%   %g%Pract: %W%{3,3}%x% |", HitRoll, DamRoll, pc.Trains, pc.Practices);
            else
                sb.AppendFormatLine("| %g%Hit:  %W%{0,6}%x%    %g%Dam:  %W%{1,6}%x% |                       |", HitRoll, DamRoll);
            sb.AppendLine("+------------------------------+-------------------------+");
            // TODO: resistances, gold, item, weight, conditions
            //if (!IS_NPC(ch) && ch->pcdata->condition[COND_DRUNK] > 10)
            //    send_to_char("You are drunk.\n\r", ch);
            //if (!IS_NPC(ch) && ch->pcdata->condition[COND_THIRST] == 0)
            //    send_to_char("You are thirsty.\n\r", ch);
            //if (!IS_NPC(ch) && ch->pcdata->condition[COND_HUNGER] == 0)
            //    send_to_char("You are hungry.\n\r", ch);
            // positions
            //switch (ch->position)
            //{
            //    case POS_DEAD:
            //        send_to_char("You are DEAD!!\n\r", ch);
            //        break;
            //    case POS_MORTAL:
            //        send_to_char("You are mortally wounded.\n\r", ch);
            //        break;
            //    case POS_INCAP:
            //        send_to_char("You are incapacitated.\n\r", ch);
            //        break;
            //    case POS_STUNNED:
            //        send_to_char("You are stunned.\n\r", ch);
            //        break;
            //    case POS_SLEEPING:
            //        send_to_char("You are sleeping.\n\r", ch);
            //        break;
            //    case POS_RESTING:
            //        send_to_char("You are resting.\n\r", ch);
            //        break;
            //    case POS_SITTING:
            //        send_to_char("You are sitting.\n\r", ch);
            //        break;
            //    case POS_STANDING:
            //        send_to_char("You are standing.\n\r", ch);
            //        break;
            //    case POS_FIGHTING:
            //        send_to_char("You are fighting.\n\r", ch);
            //        break;
            //}

            Send(sb);

            //Name[Form]

            //Attributes
            //Str / Race
            //Agi / Class
            //Int / Sex
            //Sta / Level
            //Spi / Xp2Level

            //Resource Offensive
            //Hp / AP or SP
            //Mana1 / AS
            //[Mana2] / Critical

            //Defensive
            //Armor
            //Dodge
            //Parry
            //Block

            //ComplexTableGenerator generator = new ComplexTableGenerator();
            //generator.SetColumns(10); // dummy value
            //generator.AddSeparator();
            //if (ImpersonatedBy != null)
            //    generator.AddRow(DisplayName + " (" + ImpersonatedBy.DisplayName + ")");
            //else
            //    generator.AddRow(DisplayName);
            //generator.SetColumns(25, 25);
            //generator.AddRow();
            //sb = generator.Generate();
            //Send(sb);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("where", "Information", MinPosition = Positions.Resting)]
        [Syntax(
            "[cmd]",
            "[cmd] <player name>")]
        protected virtual CommandExecutionResults DoWhere(string rawParameters, params CommandParameter[] parameters)
        {
            if (Room == null)
                return CommandExecutionResults.Error;
            if (Room.RoomFlags.HasFlag(RoomFlags.NoWhere))
            {
                Send("You don't recognize where you are.");
                return CommandExecutionResults.InvalidTarget;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormatLine($"[{Room.Area.DisplayName}].");
            //
            IEnumerable<IPlayableCharacter> playableCharacters;
            string notFound;
            if (parameters.Length == 0)
            {
                sb.AppendLine("Players near you:");
                playableCharacters = Room.Area.PlayableCharacters.Where(CanSee);
                notFound = "None";
            }
            else
            {
                playableCharacters = Room.Area.PlayableCharacters.Where(x => x.Room != null
                                                                             && !x.Room.RoomFlags.HasFlag(RoomFlags.NoWhere)
                                                                             && !x.Room.IsPrivate
                                                                             && !x.CharacterFlags.HasFlag(CharacterFlags.Sneak) 
                                                                             && !x.CharacterFlags.HasFlag(CharacterFlags.Hide) 
                                                                             && CanSee(x)
                                                                             && StringCompareHelpers.StringListsStartsWith(x.Keywords, parameters[0].Tokens));
                notFound = $"You didn't find any {parameters[0]}.";
            }
            bool found = false;
            foreach (IPlayableCharacter playableCharacter in playableCharacters)
            {
                sb.AppendFormatLine("{0,-28} {1}", playableCharacter.DisplayName, playableCharacter.Room.DisplayName);
                found = true;
            }
            if (!found)
                sb.AppendLine(notFound);
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("inventory", "Information")]
        protected virtual CommandExecutionResults DoInventory(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("You are carrying:");
            AppendItems(sb, Inventory, true, true);
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("equipment", "Information")]
        protected virtual CommandExecutionResults DoEquipment(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("You are using:");
            if (Equipments.All(x => x.Item == null))
                sb.AppendLine("Nothing");
            else
            {
                foreach (EquippedItem equippedItem in Equipments)
                {
                    string where = EquipmentSlotsToString(equippedItem);
                    sb.Append(where);
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (equippedItem.Item == null)
                        sb.AppendLine("nothing");
                    else
                        sb.AppendLine(FormatItem(equippedItem.Item, true));
                }
            }

            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("consider", "Information", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <character>")]
        protected virtual CommandExecutionResults DoConsider(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Consider killing whom?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            ICharacter whom = FindHelpers.FindByName(Room.People, parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            if (whom.IsSafe(this))
            {
                Send("Don't even think about it.");
                return CommandExecutionResults.InvalidTarget;
            }

            if (whom == this)
            {
                Send("You are such a badass.");
                return CommandExecutionResults.InvalidTarget;
            }

            int diff = whom.Level - Level;
            if (diff <= -10)
                Act(ActOptions.ToCharacter, "You can kill {0} naked and weaponless.", whom);
            else if (diff <= -5)
                Act(ActOptions.ToCharacter, "{0:N} is no match for you.", whom);
            else if (diff <= -2)
                Act(ActOptions.ToCharacter, "{0:N} looks like an easy kill.", whom);
            else if (diff <= 1)
                Send("The perfect match!");
            else if (diff <= 4)
                Act(ActOptions.ToCharacter, "{0:N} says 'Do you fell lucky punk?'.", whom);
            else if (diff <= 9)
                Act(ActOptions.ToCharacter, "{0:N} laughs at you mercilessly.", whom);
            else
                Send("Death will thank you for your gift.");
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("weather", "Information", MinPosition = Positions.Resting)]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoWeather(string rawParameters, params CommandParameter[] parameters)
        {
            if (Room == null)
                return CommandExecutionResults.Error;
            if (Room.RoomFlags.HasFlag(RoomFlags.Indoors))
            {
                Send("You can't see the weather indoors.");
                return CommandExecutionResults.InvalidTarget;
            }

            string change = TimeManager.PressureChange >= 0
                ? "a warm southerly breeze blows"
                : "a cold northern gust blows";
            Send("The sky is {0} and {1}.", TimeManager.SkyState.PrettyPrint(), change);

            if (TimeManager.IsMoonNight())
            {
                for (int i = 0; i < TimeManager.MoonCount; i++)
                    if (TimeManager.IsMoonVisible(i))
                        Send(TimeManager.MoonInfo(i));
            }

            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("time", "Information")]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoTime(string rawParameters, params CommandParameter[] parameters)
        {
            Send(TimeManager.TimeInfo());
            return CommandExecutionResults.Ok;
        }

        // Helpers
        private void AppendRoom(StringBuilder sb, IRoom room) // equivalent to act_info.C:do_look("auto")
        {
            // Room name
            if (this is IPlayableCharacter playableCharacter && playableCharacter.ImpersonatedBy is IAdmin)
                sb.AppendFormatLine($"%c%{room.DisplayName} [{room.Blueprint?.Id.ToString() ?? "???"}]%x%");
            else
                sb.AppendFormatLine("%c%{0}%x%", room.DisplayName);
            // Room description
            sb.Append(room.Description);
            // Exits
            AppendExits(sb, true);
            AppendItems(sb, Room.Content.Where(CanSee), false, false);
            AppendCharacters(sb, Room);
        }

        private void AppendCharacters(StringBuilder sb, IRoom room)
        {
            foreach (ICharacter victim in room.People.Where(x => x != this))
            {
                //  (see act_info.C:714 show_char_to_char)
                if (CanSee(victim)) // see act_info.C:375 show_char_to_char_0)
                    AppendCharacterInRoom(sb, victim);
                else if (room.IsDark && victim.CharacterFlags.HasFlag(CharacterFlags.Infrared))
                    sb.AppendLine("You see glowing red eyes watching YOU!");
            }
        }

        private void AppendContainerContent(StringBuilder sb, IContainer container)
        {
            sb.AppendFormatLine("{0} holds:", container.RelativeDisplayName(this));
            AppendItems(sb, container.Content, true, true);
        }

        private void AppendCharacterInRoom(StringBuilder sb, ICharacter victim)
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
            sb.Append(victim.RelativeDisplayName(this));
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
                    if (Furniture != null)
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
                    else if (victim.Fighting == this)
                        sb.Append("YOU!");
                    else if (victim.Room == victim.Fighting.Room)
                        sb.AppendFormat("{0}.", victim.Fighting.RelativeDisplayName(this));
                    else
                    {
                        Log.Default.WriteLine(LogLevels.Warning, "{0} is fighting {1} in a different room.", victim.DebugName, victim.Fighting.DebugName);
                        sb.Append("someone who left??");
                    }
                    break;
            }
            sb.AppendLine();
        }

        private void AppendPositionFurniture(StringBuilder sb, string verb, IItemFurniture furniture)
        {
            if (furniture == null)
                sb.AppendFormat(" is {0} here.", verb);
            else
            {
                if (Furniture.FurniturePlacePreposition == FurniturePlacePrepositions.At)
                    sb.AppendFormat(" is {0} at {1}", verb, Furniture.DisplayName);
                else if (Furniture.FurniturePlacePreposition == FurniturePlacePrepositions.On)
                    sb.AppendFormat(" is {0} on {1}", verb, Furniture.DisplayName);
                else if (Furniture.FurniturePlacePreposition == FurniturePlacePrepositions.In)
                    sb.AppendFormat(" is {0} in {1}", verb, Furniture.DisplayName);
                else
                    sb.AppendFormat(" is {0} here.", verb);
            }
        }

        private void AppendCharacter(StringBuilder sb, ICharacter victim, bool peekInventory) // equivalent to act_info.C:show_char_to_char_1
        {
            //
            string condition = "is here.";
            int maxHitPoints = victim.MaxHitPoints;
            if (maxHitPoints > 0)
            {
                int percent = (100*victim.HitPoints)/ maxHitPoints;
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
            sb.AppendLine($"{victim.RelativeDisplayName(this)} {condition}");

            //
            if (victim.Equipments.Any(x => x.Item != null))
            {
                sb.AppendLine($"{victim.RelativeDisplayName(this)} is using:");
                foreach (EquippedItem equippedItem in victim.Equipments.Where(x => x.Item != null))
                {
                    string where = EquipmentSlotsToString(equippedItem);
                    string what = FormatItem(equippedItem.Item, true);
                    sb.AppendLine($"{where}{what}");
                }
            }

            if (peekInventory)
            {
                sb.AppendLine("You peek at the inventory:");
                IEnumerable<IItem> items = this == victim
                    ? victim.Inventory
                    : victim.Inventory.Where(CanSee); // don't display 'invisible item' when inspecting someone else
                AppendItems(sb, items, true, true);
            }
        }

        private void AppendItems(StringBuilder sb, IEnumerable<IItem> items, bool shortDisplay, bool displayNothing) // equivalent to act_info.C:show_list_to_char
        {
            var enumerable = items as IItem[] ?? items.ToArray();
            if (displayNothing && !enumerable.Any())
                sb.AppendLine("Nothing.");
            else
            {
                // Grouped by description
                foreach(var groupedFormattedItem in enumerable.Select(item => FormatItem(item, shortDisplay)).GroupBy(x => x))
                {
                    int count = groupedFormattedItem.Count();
                    if (count > 1)
                        sb.AppendFormatLine("%W%({0,2})%x% {1}", count, groupedFormattedItem.Key);
                    else
                        sb.AppendFormatLine("     {0}", groupedFormattedItem.Key);
                }
            }
        }

        private void AppendExits(StringBuilder sb, bool compact)
        {
            if (compact)
                sb.Append("[Exits:");
            else
                sb.AppendLine("Obvious exits:");
            bool exitFound = false;
            foreach (ExitDirections direction in EnumHelpers.GetValues<ExitDirections>())
            {
                IExit exit = Room[direction];
                IRoom destination = exit?.Destination;
                if (destination != null && CanSee(exit))
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
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
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
                        if (this is IPlayableCharacter playableCharacter && playableCharacter.ImpersonatedBy is IAdmin)
                            sb.Append($" [{exit.Destination.Blueprint?.Id.ToString() ?? "???"}]");
                        sb.AppendLine();
                    }
                    exitFound = true;
                }
            }
            if (!exitFound)
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (compact)
                    sb.AppendLine(" none");
                else
                    sb.AppendLine("None.");
            }
            if (compact)
                sb.AppendLine("]");
        }

        private bool FindItemByExtraDescriptionOrName(CommandParameter parameter, out string description) // Find by extra description then name (search in inventory, then equipment, then in room)
        {
            description = null;
            int count = 0;
            foreach (IItem item in Inventory.Where(CanSee)
                .Concat(Equipments.Where(x => x.Item != null && CanSee(x.Item)).Select(x => x.Item))
                .Concat(Room.Content.Where(CanSee)))
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
                    description = FormatItem(item, false) + Environment.NewLine;
                    return true;
                }
            }
            return false;
        }

        //// Following method should be equivalent to FindItemByExtraDescriptionOrName
        //public string Test2(CommandParameter parameter)
        //{
        //    KeyValuePair<string, string> description = Content.Where(CanSee)
        //        .Concat(Equipments.Where(x => x.Item != null && CanSee(x.Item)).Select(x => x.Item))
        //        .Concat(Room.Content.Where(CanSee))
        //        .SelectMany(item =>
        //            (item.ExtraDescriptions ?? Enumerable.Empty<KeyValuePair<string, string>>())
        //                .Concat(item.Keywords.Select(k => new KeyValuePair<string, string>(k, item.RelativeDescription(this) + Environment.NewLine))))
        //        .Where(kv => parameter.Tokens.All(t => FindHelpers.StringStartsWith(kv.Key, t)))
        //        .ElementAtOrDefault(parameter.Count - 1);
        //    if (description.Equals(default(KeyValuePair<string, string>)))
        //        return null;
        //    return description.Value;
        //}

        private StringBuilder ScanRoom(IRoom room)
        {
            StringBuilder peopleInRoom = new StringBuilder();
            foreach (ICharacter victim in room.People.Where(CanSee))
                peopleInRoom.AppendFormatLine(" - {0}", victim.RelativeDisplayName(this));
            return peopleInRoom;
        }

        private string FormatItem(IItem item, bool shortDisplay) // TODO: (see act_info.C:170 format_obj_to_char)
        {
            // TODO: StringBuilder as param ?
            StringBuilder sb = new StringBuilder();
            // Weapon flags
            if (item is IItemWeapon weapon)
            {
                if (weapon.WeaponFlags.HasFlag(WeaponFlags.Flaming)) sb.Append("%R%(Flaming)%x%");
                if (weapon.WeaponFlags.HasFlag(WeaponFlags.Frost)) sb.Append("%C%(Frost)%x%");
                if (weapon.WeaponFlags.HasFlag(WeaponFlags.Vampiric)) sb.Append("%D%(Vampiric)%x%");
                if (weapon.WeaponFlags.HasFlag(WeaponFlags.Sharp)) sb.Append("%B%(Sharp)%x%");
                if (weapon.WeaponFlags.HasFlag(WeaponFlags.Vorpal)) sb.Append("%B%(Vorpal)%x%");
                // Two-handed not handled
                if (weapon.WeaponFlags.HasFlag(WeaponFlags.Shocking)) sb.Append("%Y%(Sparkling)%x%");
                if (weapon.WeaponFlags.HasFlag(WeaponFlags.Poison)) sb.Append("%G%(Envenomed)%x%");
                if (weapon.WeaponFlags.HasFlag(WeaponFlags.Holy)) sb.Append("%C%(Holy)%x%");
            }

            // Item flags
            if (item.ItemFlags.HasFlag(ItemFlags.Invis) && CharacterFlags.HasFlag(CharacterFlags.DetectInvis))
                sb.Append("%y%(Invis)%x%");
            if (item.ItemFlags.HasFlag(ItemFlags.Evil) && CharacterFlags.HasFlag(CharacterFlags.DetectEvil))
                sb.Append("%R%(Evil)%x%");
            if (item.ItemFlags.HasFlag(ItemFlags.Bless) && CharacterFlags.HasFlag(CharacterFlags.DetectGood))
                sb.Append("%C%(Blessed)%x%");
            if (item.ItemFlags.HasFlag(ItemFlags.Magic) && CharacterFlags.HasFlag(CharacterFlags.DetectMagic))
                sb.Append("%b%(Magical)%x%");
            if (item.ItemFlags.HasFlag(ItemFlags.Glowing))
                sb.Append("%Y%(Glowing)%x%");
            if (item.ItemFlags.HasFlag(ItemFlags.Humming))
                sb.Append("%y%(Humming)%x%");

            // Description
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (shortDisplay)
                sb.Append(item.RelativeDisplayName(this));
            else
                sb.Append(item.RelativeDescription(this));

            return sb.ToString();
        }

        private string EquipmentSlotsToString(EquippedItem equippedItem)
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
                    Wiznet.Wiznet($"DoEquipment: missing WearLocation {equippedItem.Slot}", WiznetFlags.Bugs, AdminLevels.Implementor);
                    break;
            }
            return "%C%<unknown>%x%";
        }
    }
}
