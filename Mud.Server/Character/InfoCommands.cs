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
        [Command("look", "Information", Priority = 0)]
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
            // TODO: 0/ sleeping/blind/dark room (see act_info.C:1413 -> 1436)

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
                // search in room, then in inventory(unequiped), then in equipement
                IItem containerItem = FindHelpers.FindItemHere(this, parameters[1]);
                if (containerItem == null)
                {
                    Send(StringHelpers.ItemNotFound);
                    return CommandExecutionResults.TargetNotFound;
                }

                Log.Default.WriteLine(LogLevels.Debug, "DoLook(2): found in {0}", containerItem.ContainedInto.DebugName);
                IContainer container = containerItem as IContainer;
                if (container == null)
                {
                    Send("This is not a container.");
                    return CommandExecutionResults.InvalidTarget;
                }
                // TODO: drink container
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
                foreach (KeyValuePair<string, string> extraDescription in Room.ExtraDescriptions)
                {
                    if (parameters[0].Tokens.All(x => StringCompareHelpers.StringStartsWith(extraDescription.Key, x))
                            && ++count == parameters[0].Count)
                    {
                        Send(extraDescription.Value, false);
                        return CommandExecutionResults.Ok;
                    }
                }
            }
            // 6: direction
            ExitDirections direction;
            if (ExitDirectionsExtensions.TryFindDirection(parameters[0].Value, out direction))
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(6): direction");
                IExit exit = Room.Exit(direction);
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

        [Command("exits", "Information")]
        protected virtual CommandExecutionResults DoExits(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            AppendExits(sb, false);
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("examine", "Information")]
        [Syntax(
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
            if (item is IContainer container) // if container, display content
                AppendContainerContent(sbItem, container);
            else
                sbItem.AppendLine(FormatItem(item, true));
            Send(sbItem);
            return CommandExecutionResults.Ok;
        }

        [Command("scan", "Information")]
        protected virtual CommandExecutionResults DoScan(string rawParameters, params CommandParameter[] parameters)
        {
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
                    IRoom destination = currentRoom.GetRoom(direction);
                    if (destination == null)
                        break; // stop in that direction if no exit found
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

        [Command("affects", "Information")]
        protected virtual CommandExecutionResults DoAffects(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            if (_auras.Any() || _periodicAuras.Any())
            {
                sb.AppendLine("%c%You are affected by the following auras:%x%");
                // Auras
                foreach (IAura aura in _auras.Where(x => !x.AuraFlags.HasFlag(AuraFlags.Hidden)).OrderBy(x => x.PulseLeft))
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

        [Command("score", "Information", Priority = 2)]
        protected virtual CommandExecutionResults DoScore(string rawParameters, params CommandParameter[] parameters)
        {
            IPlayableCharacter pc = this as IPlayableCharacter;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("+--------------------------------------------------------+"); // length 1 + 56 + 1
            sb.AppendLine("|" + StringExtensions.CenterText(DisplayName, 56) + "|");
            sb.AppendLine("+------------------------------+-------------------------+");
            sb.AppendLine("| %W%Attributes%x%                   |                         |");
            sb.AppendFormatLine("| %c%Strength     : %W%[{0,5}/{1,5}]%x% | %c%Race   : %W%{2,14}%x% |", this[CharacterAttributes.Strength], BaseAttribute(CharacterAttributes.Strength), Race?.DisplayName ?? "(none)");
            sb.AppendFormatLine("| %c%Intelligence : %W%[{0,5}/{1,5}]%x% | %c%Class  : %W%{2,14}%x% |", this[CharacterAttributes.Intelligence], BaseAttribute(CharacterAttributes.Intelligence), Class?.DisplayName ?? "(none)");
            sb.AppendFormatLine("| %c%Wisdom       : %W%[{0,5}/{1,5}]%x% | %c%Sex    : %W%{2,14}%x% |", this[CharacterAttributes.Wisdom], BaseAttribute(CharacterAttributes.Wisdom), CurrentSex);
            sb.AppendFormatLine("| %c%Dexterity    : %W%[{0,5}/{1,5}]%x% | %c%Level  : %W%{2,14}%x% |", this[CharacterAttributes.Dexterity], BaseAttribute(CharacterAttributes.Dexterity), Level);
            if (pc != null)
                sb.AppendFormatLine("| %c%Constitution : %W%[{0,5}/{1,5}]%x% | %c%NxtLvl : %W%{2,14}%x% |", this[CharacterAttributes.Constitution], BaseAttribute(CharacterAttributes.Constitution), pc.ExperienceToLevel);
            else
                sb.AppendFormatLine("| %c%Constitution : %W%[{0,5}/{1,5}]%x% |                       |", this[CharacterAttributes.Constitution], BaseAttribute(CharacterAttributes.Constitution), Level);
            sb.AppendLine("+------------------------------+-------------------------+");
            sb.AppendLine("| %W%Resources%x%                    | %W%Defensive%x%              |");
            sb.AppendFormatLine("| %g%Hp     : %W%[{0,8}/{1,8}]%x% | %g%Bash         : %W%[{2,6}]%x% |", HitPoints, MaxHitPoints, this[CharacterAttributes.ArmorBash]);
            sb.AppendFormatLine("| %g%Move   : %W%[{0,8}/{1,8}]%x% | %g%Pierce       : %W%[{2,6}]%x% |", MovePoints, this[CharacterAttributes.MaxMovePoints], this[CharacterAttributes.ArmorPierce]);
            List<string> resources = new List<string>();
            foreach (ResourceKinds resourceKind in CurrentResourceKinds)
                resources.Add($"%g%{resourceKind,-7}: %W%[{this[resourceKind],8}/{MaxResource(resourceKind),8}]%x%");
            if (resources.Count < 3)
                resources.AddRange(Enumerable.Repeat("                            ", 3 - resources.Count));
            sb.AppendFormatLine("| {0} | %g%Slash        : %W%[{1,6}]%x% |", resources[0], this[CharacterAttributes.ArmorSlash]);
            sb.AppendFormatLine("| {0} | %g%Exotic       : %W%[{1,6}]%x% |", resources[1], this[CharacterAttributes.ArmorMagic]);
            sb.AppendFormatLine("| {0} | %g%Saves        : %W%[{1,6}]%x% |", resources[2], this[CharacterAttributes.SavingThrow]);
            sb.AppendLine("+------------------------------+-------------------------+");
            if (pc != null)
                sb.AppendFormatLine("| %g%Hit:  %W%{0,6}%x%    %g%Dam:  %W%{1,6}%x% | %g%Train: %W%{2,3}%x%   %g%Pract: %W%{3,3}%x% |", this[CharacterAttributes.HitRoll], this[CharacterAttributes.DamRoll], pc.Trains, pc.Practices);
            else
                sb.AppendFormatLine("| %g%Hit:  %W%{0,6}%x%    %g%Dam:  %W%{1,6}%x% |                       |", this[CharacterAttributes.HitRoll], this[CharacterAttributes.DamRoll]);
            sb.AppendLine("+------------------------------+-------------------------+");
            // TODO: resistances, gold, item, weight

            Send(sb);
            //
            // REDO
            // TODO: score all will display everything (every resources, every stats)
            //IPlayableCharacter playableCharacter = this as IPlayableCharacter;

            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine();
            //sb.AppendLine("+--------------------------------------------------------+"); // length = 56
            //string form = string.Empty;
            //if (Form != Forms.Normal)
            //    form = $"%m% [Form: {Form}]%x%";
            //if (playableCharacter != null)
            //    sb.AppendLine("|" + StringExtensions.CenterText(DisplayName + " (" + playableCharacter.DisplayName + ")" + form, form == string.Empty ? 56 : 62) + "|");
            //else
            //    sb.AppendLine("|" + StringExtensions.CenterText(DisplayName + form, form == string.Empty ? 56 : 62) + "|");
            //sb.AppendLine("+---------------------------+----------------------------+");
            //sb.AppendLine("| %W%Attributes%x%                |                            |");
            //sb.AppendFormatLine("| %c%Strength  : %W%[{0,5}/{1,5}]%x% | %c%Race   : %W%{2,17}%x% |", this[PrimaryAttributeTypes.Strength], GetBasePrimaryAttribute(PrimaryAttributeTypes.Strength), Race?.DisplayName ?? "(none)");
            //sb.AppendFormatLine("| %c%Agility   : %W%[{0,5}/{1,5}]%x% | %c%Class  : %W%{2,17}%x% |", this[PrimaryAttributeTypes.Agility], GetBasePrimaryAttribute(PrimaryAttributeTypes.Agility), Class?.DisplayName ?? "(none)");
            //sb.AppendFormatLine("| %c%Stamina   : %W%[{0,5}/{1,5}]%x% | %c%Sex    : %W%{2,17}%x% |", this[PrimaryAttributeTypes.Stamina], GetBasePrimaryAttribute(PrimaryAttributeTypes.Stamina), Sex);
            //sb.AppendFormatLine("| %c%Intellect : %W%[{0,5}/{1,5}]%x% | %c%Level  : %W%{2,17}%x% |", this[PrimaryAttributeTypes.Intellect], GetBasePrimaryAttribute(PrimaryAttributeTypes.Intellect), Level);
            //if (playableCharacter != null)
            //    sb.AppendFormatLine("| %c%Spirit    : %W%[{0,5}/{1,5}]%x% | %c%NxtLvl : %W%{2,17}%x% |", this[PrimaryAttributeTypes.Spirit], GetBasePrimaryAttribute(PrimaryAttributeTypes.Spirit), playableCharacter.ExperienceToLevel);
            //else
            //    sb.AppendFormatLine("| %c%Spirit    : %W%[{0,5}/{1,5}]%x% |                       |", this[PrimaryAttributeTypes.Spirit], GetBasePrimaryAttribute(PrimaryAttributeTypes.Spirit));
            //sb.AppendLine("+---------------------------+--+-------------------------+");
            //sb.AppendLine("| %W%Resources%x%                    | %W%Offensive%x%               |");
            //// TODO: don't display both Attack Power and Spell Power
            //sb.AppendFormatLine("| %g%Hit    : %W%[{0,8}/{1,8}]%x% | %g%Move  : %W%[{2,6}/{3,6}]%x% |", HitPoints, this[SecondaryAttributeTypes.MaxHitPoints], MovePoints, this[SecondaryAttributeTypes.MaxMovePoints]);
            //List<string> resources = CurrentResourceKinds.Fill(4).Select(x => x == ResourceKinds.None
            //    ? "                            "
            //    : $"%g%{x,-7}:     %W%[{this[x],6}/{GetMaxResource(x),6}]%x%").ToList();
            //sb.AppendFormatLine("| {0} | %g%Attack Power : %W%[{1,6}]%x% |", resources[0], this[SecondaryAttributeTypes.AttackPower]);
            //sb.AppendFormatLine("| {0} | %g%Spell Power  : %W%[{1,6}]%x% |", resources[1], this[SecondaryAttributeTypes.SpellPower]);
            //sb.AppendFormatLine("| {0} | %g%Attack Speed : %W%[{1,6}]%x% |", resources[2], this[SecondaryAttributeTypes.AttackSpeed]);
            //sb.AppendFormatLine("| {0} | %g%Armor        : %W%[{1,6}]%x% |", resources[3], this[SecondaryAttributeTypes.Armor]);
            //sb.AppendLine("+------------------------------+-------------------------+");
            //sb.AppendLine("| %W%Defensive%x%                    |                         |");
            //sb.AppendFormatLine("| %g%Dodge              : %W%[{0,5}]%x% |                         |", this[SecondaryAttributeTypes.Dodge]);
            //sb.AppendFormatLine("| %g%Parry              : %W%[{0,5}]%x% |                         |", this[SecondaryAttributeTypes.Parry]);
            //sb.AppendFormatLine("| %g%Block              : %W%[{0,5}]%x% |                         |", this[SecondaryAttributeTypes.Block]);
            //sb.AppendLine("+------------------------------+-------------------------+");
            //// TODO: resistances, gold, item, weight
            //Send(sb);

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

        [Command("where", "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <player name>")]
        protected virtual CommandExecutionResults DoWhere(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormatLine($"[{Room.Area.DisplayName}].");
            //
            IEnumerable<IPlayer> players;
            string notFound;
            if (parameters.Length == 0)
            {
                sb.AppendLine("Players near you:");
                players = Room.Area.Players.Where(x => CanSee(x.Impersonating));
                notFound = "None";
            }
            else
            {
                players = Room.Area.Players.Where(x => CanSee(x.Impersonating) && StringCompareHelpers.StringListsStartsWith(x.Impersonating.Keywords, parameters[0].Tokens));
                notFound = $"You didn't find any {parameters[0]}.";
            }
            bool found = false;
            foreach (IPlayer player in players)
            {
                sb.AppendFormatLine("{0,-28} {1}", player.Impersonating.DisplayName, player.Impersonating.Room.DisplayName);
                found = true;
            }
            if (!found)
                sb.AppendLine(notFound);
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("inventory", "Information")]
        protected virtual CommandExecutionResults DoInventory(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("You are carrying:");
            AppendItems(sb, Inventory, true, true);
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("equipment", "Information")]
        protected virtual CommandExecutionResults DoEquipment(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("You are using:");
            if (Equipments.All(x => x.Item == null))
                sb.AppendLine("Nothing");
            else
            {
                foreach (EquipedItem equipedItem in Equipments)
                {
                    string where = EquipmentSlotsToString(equipedItem);
                    sb.Append(where);
                    if (equipedItem.Item == null)
                        sb.AppendLine("nothing");
                    else
                        sb.AppendLine(FormatItem(equipedItem.Item, true));
                }
            }

            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("consider", "Information")]
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

            if (whom == this)
            {
                Send("You are such a badass.");
                return CommandExecutionResults.InvalidTarget;
            }

            CombatHelpers.CombatDifficulties difficulty = CombatHelpers.GetConColor(Level, whom.Level);

            switch (difficulty)
            {
                case CombatHelpers.CombatDifficulties.Grey:
                    Act(ActOptions.ToCharacter, "You can kill {0} naked and weaponless.", whom);
                    break;
                case CombatHelpers.CombatDifficulties.Green:
                    Act(ActOptions.ToCharacter, "{0:N} looks like an easy kill.", whom);
                    break;
                case CombatHelpers.CombatDifficulties.Yellow:
                    Send("The perfect match!");
                    break;
                case CombatHelpers.CombatDifficulties.Orange:
                    Act(ActOptions.ToCharacter, "{0:N} says 'Do you fell lucky punk?'.", whom);
                    break;
                case CombatHelpers.CombatDifficulties.Red:
                    Act(ActOptions.ToCharacter, "{0:N} laughs at you mercilessly.", whom);
                    break;
                case CombatHelpers.CombatDifficulties.Skull:
                    Send("Death will thank you for your gift.");
                    break;
                default:
                    Act(ActOptions.ToCharacter, "You failed to consider killing {0}.", whom);
                    Log.Default.WriteLine(LogLevels.Error, "DoConsider: unhandled CombatDifficulties: {0}", difficulty);
                    break;
            }

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
            foreach (ICharacter victim in Room.People.Where(x => x != this))
            {
                //  (see act_info.C:714 show_char_to_char)
                if (CanSee(victim)) // see act_info.C:375 show_char_to_char_0)
                {
                    // TODO: display flags (see act_info.C:387 -> 478)
                    // TODO: display long description and stop
                    // TODO: display position (see act_info.C:505 -> 612)

                    // last case of POS_STANDING
                    sb.Append(victim.RelativeDisplayName(this));
                    switch (victim.Position)
                    {
                        case Positions.Stunned:
                            sb.Append(" is lying here stunned.");
                            break;
                        case Positions.Sleeping:
                            sb.Append(" is sleeping here."); // TODO: check furniture (add in ICharacter IItemFurniture On { get; } )
                            break;
                        case Positions.Resting:
                            sb.Append(" is resting here."); // TODO: check furniture (add in ICharacter IItemFurniture On { get; } )
                            break;
                        case Positions.Sitting:
                            sb.Append(" is sitting here."); // TODO: check furniture (add in ICharacter IItemFurniture On { get; } )
                            break;
                        case Positions.Standing:
                            sb.Append(" is here."); // TODO: check furniture (add in ICharacter IItemFurniture On { get; } )
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
                else
                    ; // TODO: INFRARED (see act_info.C:728)
            }
        }

        private void AppendContainerContent(StringBuilder sb, IContainer container)
        {
            // TODO: check if closed
            sb.AppendFormatLine("{0} holds:", container.RelativeDisplayName(this));
            AppendItems(sb, container.Content, true, true);
        }

        private void AppendCharacter(StringBuilder sb, ICharacter victim, bool peekInventory) // equivalent to act_info.C:show_char_to_char_1
        {
            //
            string condition = "is here.";
            int maxHitPoints = victim[CharacterAttributes.MaxHitPoints];
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
                foreach (EquipedItem equipedItem in victim.Equipments.Where(x => x.Item != null))
                {
                    string where = EquipmentSlotsToString(equipedItem);
                    string what = FormatItem(equipedItem.Item, true);
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
                IExit exit = Room.Exit(direction);
                if (exit?.Destination != null && CanSee(exit))
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
                        sb.Append(StringExtensions.UpperFirstLetter(direction.ToString()));
                        sb.Append(" - ");
                        if (exit.IsClosed)
                            sb.Append("A closed door");
                        else
                            sb.Append(exit.Destination.DisplayName); // TODO: too dark to tell
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
                    foreach (KeyValuePair<string, string> extraDescription in item.ExtraDescriptions)
                        if (parameter.Tokens.All(x => StringCompareHelpers.StringStartsWith(extraDescription.Key, x))
                            && ++count == parameter.Count)
                        {
                            description = extraDescription.Value;
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
                peopleInRoom.AppendFormatLine(" - {0}", victim.RelativeDisplayName(this)); // TODO: use RelativeDisplayName ???
            return peopleInRoom;
        }

        private string FormatItem(IItem item, bool shortDisplay) // TODO: (see act_info.C:170 format_obj_to_char)
        {
            // TODO: StringBuilder as param ?
            StringBuilder sb = new StringBuilder();
            // Weapon flags
            if (item is IItemWeapon weapon)
            {
                if (weapon.CurrentWeaponFlags.HasFlag(WeaponFlags.Flaming)) sb.Append("%R%(Flaming)%x%");
                if (weapon.CurrentWeaponFlags.HasFlag(WeaponFlags.Frost)) sb.Append("%C%(Frost)%x%");
                if (weapon.CurrentWeaponFlags.HasFlag(WeaponFlags.Vampiric)) sb.Append("%D%(Vampiric)%x%");
                if (weapon.CurrentWeaponFlags.HasFlag(WeaponFlags.Sharp)) sb.Append("%B%(Sharp)%x%");
                if (weapon.CurrentWeaponFlags.HasFlag(WeaponFlags.Vorpal)) sb.Append("%B%(Vorpal)%x%");
                // Two-handed not handled
                if (weapon.CurrentWeaponFlags.HasFlag(WeaponFlags.Shocking)) sb.Append("%Y%(Sparkling)%x%");
                if (weapon.CurrentWeaponFlags.HasFlag(WeaponFlags.Poison)) sb.Append("%G%(Envenomed)%x%");
                if (weapon.CurrentWeaponFlags.HasFlag(WeaponFlags.Holy)) sb.Append("%C%(Holy)%x%");
            }

            // Item flags
            if (item.CurrentItemFlags.HasFlag(ItemFlags.Invis)) // TODO: and detect invis
                sb.Append("%y%(Invis)%x%");
            if (item.CurrentItemFlags.HasFlag(ItemFlags.Evil))// TODO: and detect evil
                sb.Append("%R%(Evil)%x%");
            if (item.CurrentItemFlags.HasFlag(ItemFlags.Bless)) // TODO: and detect good
                sb.Append("%C%(Blessed)%x%");
            if (item.CurrentItemFlags.HasFlag(ItemFlags.Magic)) // TODO: and detect magic
                sb.Append("%b%(Magical)%x%");
            if (item.CurrentItemFlags.HasFlag(ItemFlags.Glowing))
                sb.Append("%Y%(Glowing)%x%");
            if (item.CurrentItemFlags.HasFlag(ItemFlags.Humming))
                sb.Append("%y%(Humming)%x%");

            // Description
            if (shortDisplay)
                sb.Append(item.RelativeDisplayName(this));
            else
                sb.Append(item.RelativeDescription(this));

            return sb.ToString();
        }

        private string EquipmentSlotsToString(EquipedItem equipedItem)
        {
            switch (equipedItem.Slot)
            {
                case EquipmentSlots.Light:
                    return "%C%<used as light>          %x%";
                case EquipmentSlots.Head:
                    return "%C%<worn on head>           %x%";
                case EquipmentSlots.Amulet:
                    return "%C%<worn on neck>           %x%";
                case EquipmentSlots.Shoulders:
                    return "%C%<worn around shoulders>  %x%";
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
                case EquipmentSlots.Trinket:
                    return "%C%<worn as trinket>        %x%";
                case EquipmentSlots.MainHand:
                    return "%C%<wielded>                %x%";
                case EquipmentSlots.OffHand:
                    if (equipedItem.Item != null)
                    {
                        if (equipedItem.Item is IItemShield)
                            return "%C%<worn as shield>         %x%";
                        if (equipedItem.Item.WearLocation == WearLocations.Hold)
                            return "%C%<held>                   %x%";
                    }
                    return "%c%<offhand>                %x%";
                default:
                    Log.Default.WriteLine(LogLevels.Error, "DoEquipment: missing WearLocation {0}", equipedItem.Slot);
                    break;
            }
            return "%C%<unknown>%x%";
        }
    }
}
