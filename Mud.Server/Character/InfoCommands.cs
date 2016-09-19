using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;

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
        [Command("look", Category = "Information", Priority = 0)]
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
                    Send("Look in what?");
                else
                {
                    // search in room, then in inventory(unequiped), then in equipement
                    IItem containerItem = FindHelpers.FindItemHere(this, parameters[1]);
                    if (containerItem == null)
                        Send(StringHelpers.ItemNotFound);
                    else
                    {
                        Log.Default.WriteLine(LogLevels.Debug, "DoLook(2): found in {0}", containerItem.ContainedInto.DebugName);
                        IContainer container = containerItem as IContainer;
                        if (container != null)
                            DisplayContainerContent(container);
                        // TODO: drink container
                        else
                            Send("This is not a container.");
                    }
                }
                return true;
            }
            // 3: character in room
            ICharacter victim = FindHelpers.FindByName(Room.People.Where(CanSee), parameters[0]);
            if (victim != null)
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(3): character in room");
                DisplayCharacter(victim, true); // TODO: always peeking ???
                return true;
            }
            // 4:search among inventory/equipment/room.content if an item has extra description or name equals to parameters
            string itemDescription;
            bool itemFound = FindItemByExtraDescriptionOrName(parameters[0], out itemDescription);
            if (itemFound)
            {
                Log.Default.WriteLine(LogLevels.Debug, "DoLook(4): item in inventory+equipment+room -> {0}", itemDescription);
                Send(itemDescription, false);
                return true;
            }
            // 5: extra description in room
            if (Room.ExtraDescriptions != null && Room.ExtraDescriptions.Any())
            {
                // TODO: try to use ElementAtOrDefault
                int count = 0;
                foreach (KeyValuePair<string, string> extraDescription in Room.ExtraDescriptions)
                {
                    if (parameters[0].Tokens.All(x => FindHelpers.StringStartsWith(extraDescription.Key, x))
                            && ++count == parameters[0].Count)
                    {
                        Send(extraDescription.Value, false);
                        return true;
                    }
                }
            }
            // 6: direction
            ExitDirections direction;
            if (ExitHelpers.FindDirection(parameters[0].Value, out direction))
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
                        if (exit.IsClosed)
                            Act(ActOptions.ToCharacter, "The {0} is closed.", exit);
                        else if (exit.IsDoor)
                            Act(ActOptions.ToCharacter, "The {0} is open.", exit);
                    }
                }
            }
            else
                Send(StringHelpers.ItemNotFound);
            return true;
        }

        [Command("exits", Category = "Information")]
        protected virtual bool DoExits(string rawParameters, params CommandParameter[] parameters)
        {
            DisplayExits(false);
            return true;
        }

        [Command("examine", Category = "Information")]
        protected virtual bool DoExamine(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Examine what or whom?");
            else
            {
                ICharacter victim = FindHelpers.FindByName(Room.People, parameters[0]);
                if (victim != null)
                {
                    Act(ActOptions.ToAll, "{0:N} examine{0:v} {1}.", this, victim);
                    DisplayCharacter(victim, true);
                    // TODO: display race and size
                }
                else
                {
                    IItem item = FindHelpers.FindItemHere(this, parameters[0]);
                    if (item != null)
                    {
                        Act(ActOptions.ToAll, "{0:N} examine{0:v} {1}.", this, item);
                        DisplayItem(item);
                        IContainer container = item as IContainer;
                        if (container != null) // if container, display content
                            DisplayContainerContent(container);
                    }
                    else
                        Send("You don't see any {0}.", parameters[0]);
                }
            }
            return true;
        }

        [Command("scan", Category = "Information")]
        protected virtual bool DoScan(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder(1024);
            // Current room
            sb.AppendLine("Right here you see:");
            //Send("Right here you see:");
            StringBuilder currentScan = ScanRoom(Room);
            if (currentScan.Length == 0)
                //Send("None"); // should never happen, 'this' is in the room
                sb.AppendLine("None");
            else
            //Send(currentScan); // no need to add CRLF
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
                        //Send(roomScan); // no need to add CRLF
                        sb.Append(roomScan);
                    }
                    currentRoom = destination;
                }
            }
            Send(sb);
            return true;
        }

        [Command("affects", Category = "Information")]
        protected virtual bool DoAffects(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            if (_auras.Any() || _periodicAuras.Any())
            {
                sb.AppendLine("%c%You are affected by the following auras:%x%");
                // Auras
                foreach (IAura aura in _auras.Where(x => x.Ability == null || (x.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden))
                {
                    if (aura.Modifier == AuraModifiers.None)
                        sb.AppendFormatLine("%B%{0}%x% for %c%{1}%x%",
                            aura.Ability == null ? "Unknown" : aura.Ability.Name,
                            StringHelpers.FormatDelay(aura.SecondsLeft));
                    else
                        sb.AppendFormatLine("%B%{0}%x% modifies %W%{1}%x% by %m%{2}{3}%x% for %c%{4}%x%",
                            aura.Ability == null ? "Unknown" : aura.Ability.Name,
                            aura.Modifier,
                            aura.Amount,
                            aura.AmountOperator == AmountOperators.Fixed ? String.Empty : "%",
                            StringHelpers.FormatDelay(aura.SecondsLeft));
                }
                // Periodic auras
                foreach (IPeriodicAura pa in _periodicAuras.Where(x => x.Ability == null || (x.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden))
                {
                    if (pa.AuraType == PeriodicAuraTypes.Damage)
                        sb.AppendFormatLine("%B%{0}%x% %W%deals {1}{2}%x% {3} damage every %g%{4}%x% for %c%{5}%x%",
                            pa.Ability == null ? "Unknown" : pa.Ability.Name,
                            pa.Amount,
                            pa.AmountOperator == AmountOperators.Fixed ? String.Empty : "%",
                            StringHelpers.SchoolTypeColor(pa.School),
                            StringHelpers.FormatDelay(pa.TickDelay),
                            StringHelpers.FormatDelay(pa.SecondsLeft));
                    else
                        sb.AppendFormatLine("%B%{0}%x% %W%heals {1}{2}%x% hp every %g%{3}%x% for %c%{4}%x%",
                            pa.Ability == null ? "Unknown" : pa.Ability.Name,
                            pa.Amount,
                            pa.AmountOperator == AmountOperators.Fixed ? String.Empty : "%",
                            StringHelpers.FormatDelay(pa.TickDelay),
                            StringHelpers.FormatDelay(pa.SecondsLeft));
                }
            }
            else
                sb.AppendLine("%c%You are not affected by any spells.%x%");
            Send(sb);
            return true;
        }

        [Command("score", Category = "Information", Priority = 2)]
        protected virtual bool DoScore(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: score all will display everything (every resources, every stats)

            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("+--------------------------------------------------------+"); // length = 56
            string form = String.Empty;
            if (Form != Forms.Normal)
                form = $"%m% [Form: {Form}]%x%";
            if (ImpersonatedBy != null)
                sb.AppendLine("|" + StringHelpers.CenterText(DisplayName + " (" + ImpersonatedBy.DisplayName + ")" + form, form == String.Empty ? 56 : 62) + "|");
            else
                sb.AppendLine("|" + StringHelpers.CenterText(DisplayName + form, form == String.Empty ? 56 : 62) + "|");
            sb.AppendLine("+---------------------------+----------------------------+");
            sb.AppendLine("| %W%Attributes%x%                |                            |");
            sb.AppendFormatLine("| %c%Strength  : %W%[{0,5}/{1,5}]%x% | %c%Race   : %W%{2,17}%x% |", this[PrimaryAttributeTypes.Strength], GetBasePrimaryAttribute(PrimaryAttributeTypes.Strength), Race?.DisplayName ?? "(none)");
            sb.AppendFormatLine("| %c%Agility   : %W%[{0,5}/{1,5}]%x% | %c%Class  : %W%{2,17}%x% |", this[PrimaryAttributeTypes.Agility], GetBasePrimaryAttribute(PrimaryAttributeTypes.Agility), Class?.DisplayName ?? "(none)");
            sb.AppendFormatLine("| %c%Stamina   : %W%[{0,5}/{1,5}]%x% | %c%Sex    : %W%{2,17}%x% |", this[PrimaryAttributeTypes.Stamina], GetBasePrimaryAttribute(PrimaryAttributeTypes.Stamina), Sex);
            sb.AppendFormatLine("| %c%Intellect : %W%[{0,5}/{1,5}]%x% | %c%Level  : %W%{2,17}%x% |", this[PrimaryAttributeTypes.Intellect], GetBasePrimaryAttribute(PrimaryAttributeTypes.Intellect), Level);
            sb.AppendFormatLine("| %c%Spirit    : %W%[{0,5}/{1,5}]%x% | %c%NxtLvl : %W%{2,17}%x% |", this[PrimaryAttributeTypes.Spirit], GetBasePrimaryAttribute(PrimaryAttributeTypes.Spirit), ExperienceToLevel);
            sb.AppendLine("+---------------------------+--+-------------------------+");
            sb.AppendLine("| %W%Resources%x%                    | %W%Offensive%x%               |");
            // TODO: don't display both Attack Power and Spell Power
            sb.AppendFormatLine("| %g%Hit    : %W%[{0,8}/{1,8}]%x% | %g%Attack Power : %W%[{2,6}]%x% |", HitPoints, this[SecondaryAttributeTypes.MaxHitPoints], this[SecondaryAttributeTypes.AttackPower]);
            List<string> resources = CurrentResourceKinds.Fill(3).Select(x => x == ResourceKinds.None
                ? "                            "
                : $"%g%{x,-7}:     %W%[{this[x],6}/{GetMaxResource(x),6}]%x%").ToList();
            sb.AppendFormatLine("| {0} | %g%Spell Power  : %W%[{1,6}]%x% |", resources[0], this[SecondaryAttributeTypes.SpellPower]);
            sb.AppendFormatLine("| {0} | %g%Attack Speed : %W%[{1,6}]%x% |", resources[1], this[SecondaryAttributeTypes.AttackSpeed]);
            sb.AppendFormatLine("| {0} | %g%Armor        : %W%[{1,6}]%x% |", resources[2], this[SecondaryAttributeTypes.Armor]);
            sb.AppendLine("+------------------------------+-------------------------+");
            sb.AppendLine("| %W%Defensive%x%                    |                         |");
            sb.AppendFormatLine("| %g%Dodge              : %W%[{0,5}]%x% |                         |", this[SecondaryAttributeTypes.Dodge]);
            sb.AppendFormatLine("| %g%Parry              : %W%[{0,5}]%x% |                         |", this[SecondaryAttributeTypes.Parry]);
            sb.AppendFormatLine("| %g%Block              : %W%[{0,5}]%x% |                         |", this[SecondaryAttributeTypes.Block]);
            sb.AppendLine("+------------------------------+-------------------------+");
            // TODO: resistances, gold, item, weight
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
            return true;
        }

        [Command("where", Category = "Information")]
        protected virtual bool DoWhere(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormatLine($"[{Room.Area.DisplayName}].");
            if (parameters.Length == 0)
            {
                sb.AppendLine("Peoples near you:");
                bool found = false;
                foreach (IPlayer player in Room.Area.Players.Where(x => CanSee(x.Impersonating)))
                {
                    sb.AppendFormatLine("{0,-28} {1}", player.Impersonating.DisplayName, player.Impersonating.Room.DisplayName);
                    found = true;
                }
                if (!found)
                    sb.AppendLine("None");
            }
            else
            {
                bool found = false;
                foreach (IPlayer player in Room.Area.Players.Where(x => CanSee(x.Impersonating) && FindHelpers.StringListStartsWith(x.Impersonating.Keywords, parameters[0].Tokens)))
                {
                    sb.AppendFormatLine("{0,-28} {1}", player.Impersonating.DisplayName, player.Impersonating.Room.DisplayName);
                    found = true;
                }
                if (!found)
                    sb.AppendLine($"You didn't find any {parameters[0]}.");
            }
            Send(sb);
            return true;
        }

        [Command("inventory", Category = "Information")]
        protected virtual bool DoInventory(string rawParameters, params CommandParameter[] parameters)
        {
            Send("You are carrying:");
            DisplayItems(Content, true, true);
            return true;
        }

        [Command("equipment", Category = "Information")]
        protected virtual bool DoEquipment(string rawParameters, params CommandParameter[] parameters)
        {
            Send("You are using:");
            if (Equipments.All(x => x.Item == null))
                Send("Nothing");
            else
            {
                StringBuilder sb = new StringBuilder();
                //foreach (EquipedItem equipedItem in Equipments.Where(x => x.Item != null))
                foreach (EquipedItem equipedItem in Equipments)
                {
                    string where = EquipmentSlotsToString(equipedItem.Slot);
                    sb.Append(where);
                    //sb.AppendLine(FormatItem(equipedItem.Item, true));
                    if (equipedItem.Item == null)
                        sb.AppendLine("nothing");
                    else
                        sb.AppendLine(FormatItem(equipedItem.Item, true).ToString());
                }
                Send(sb);
            }
            return true;
        }

        [Command("consider", Category = "Information")]
        protected virtual bool DoConsider(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Consider killing whom?");
                return true;
            }

            ICharacter whom = FindHelpers.FindByName(Room.People, parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
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

            return true;
        }

        // Helpers
        private void DisplayRoom() // equivalent to act_info.C:do_look("auto")
        {
            // Room name
            if (ImpersonatedBy != null && ImpersonatedBy is IAdmin)
                Send($"%c%{Room.DisplayName} [{Room.Blueprint?.Id.ToString() ?? "???"}]%x%");
            else
                Send("%c%{0}%x%", Room.DisplayName);
            // Room description
            Send(Room.Description, false); // false: don't add trailing NewLine
            // Exits
            DisplayExits(true);
            DisplayItems(Room.Content.Where(CanSee), false, false);
            foreach (ICharacter victim in Room.People.Where(x => x != this))
            {
                //  (see act_info.C:714 show_char_to_char)
                if (CanSee(victim)) // see act_info.C:375 show_char_to_char_0)
                {
                    // TODO: display flags (see act_info.C:387 -> 478)
                    // TODO: display long description and stop
                    // TODO: display position (see act_info.C:505 -> 612)

                    // last case of POS_STANDING
                    StringBuilder sb = new StringBuilder();
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
                                Log.Default.WriteLine(LogLevels.Warning, "{0} position is fighting but fighting is null.", victim.DisplayName);
                                sb.Append("thing air??");
                            }
                            else if (victim.Fighting == this)
                                sb.Append("YOU!");
                            else if (victim.Room == victim.Fighting.Room)
                                sb.AppendFormat("{0}.", victim.Fighting.RelativeDisplayName(this));
                            else
                            {
                                Log.Default.WriteLine(LogLevels.Warning, "{0} is fighting {1} in a different room.", victim.DisplayName, victim.Fighting.DisplayName);
                                sb.Append("someone who left??");
                            }
                            break;
                    }
                    sb.AppendLine();
                    //Send("{0} is here.", victim.RelativeDisplayName(this));
                    Send(sb);
                }
                else
                    ; // TODO: INFRARED (see act_info.C:728)
            }
        }

        private void DisplayContainerContent(IContainer container)
        {
            // TODO: check if closed
            Send("{0} holds:", container.DisplayName);
            DisplayItems(container.Content, true, true);
        }

        private void DisplayCharacter(ICharacter victim, bool peekInventory) // equivalent to act_info.C:show_char_to_char_1
        {
            if (this == victim)
                Act(ActOptions.ToRoom, "{0} looks at {0:m}self.", this);
            else
            {
                //victim.Act(ActOptions.ToCharacter, "{0} looks at you.", this);
                //ActToNotVictim(victim, "{0} looks at {1}.", this, victim);
                Act(ActOptions.ToRoom, "{0} looks at {1}.", this, victim);
            }
            //
            string condition = "is here.";
            if (victim[SecondaryAttributeTypes.MaxHitPoints] > 0)
            {
                int percent = (100*victim.HitPoints)/victim[SecondaryAttributeTypes.MaxHitPoints];
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
            Send($"{victim.RelativeDisplayName(this)} {condition}");

            //
            if (victim.Equipments.Any(x => x.Item != null))
            {
                Act(ActOptions.ToCharacter, "{0} is using:", victim);
                foreach (EquipedItem equipedItem in victim.Equipments.Where(x => x.Item != null))
                {
                    string where = EquipmentSlotsToString(equipedItem.Slot);
                    StringBuilder sb = new StringBuilder(where);
                    sb.AppendLine(FormatItem(equipedItem.Item, true).ToString());
                    Send(sb);
                }
            }

            if (peekInventory)
            {
                Send("You peek at the inventory:");
                IEnumerable<IItem> items = this == victim
                    ? victim.Content
                    : victim.Content.Where(CanSee); // don't display 'invisible item' when inspecting someone else
                DisplayItems(items, true, true);
            }
        }

        private void DisplayItem(IItem item)
        {
            string formattedItem = FormatItem(item, true);
            Send(formattedItem);
        }

        private void DisplayItems(IEnumerable<IItem> items, bool shortDisplay, bool displayNothing) // equivalent to act_info.C:show_list_to_char
        {
            StringBuilder sb = new StringBuilder();
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
            Send(sb);
        }

        private void DisplayExits(bool compact)
        {
            StringBuilder message = new StringBuilder();
            if (compact)
                message.Append("[Exits:");
            else
                message.AppendLine("Obvious exits:");
            bool exitFound = false;
            foreach (ExitDirections direction in EnumHelpers.GetValues<ExitDirections>())
            {
                IExit exit = Room.Exit(direction);
                if (exit?.Destination != null && CanSee(exit))
                {
                    if (compact)
                    {
                        message.Append(" ");
                        if (exit.IsHidden)
                            message.Append("[");
                        if (exit.IsClosed)
                            message.Append("(");
                        message.AppendFormat("{0}", direction.ToString().ToLowerInvariant());
                        if (exit.IsClosed)
                            message.Append(")");
                        if (exit.IsHidden)
                            message.Append("]");
                    }
                    else
                    {
                        message.Append(StringHelpers.UpperFirstLetter(direction.ToString()));
                        message.Append(" - ");
                        if (exit.IsClosed)
                            message.Append("A closed door");
                        else
                            message.Append(exit.Destination.DisplayName); // TODO: too dark to tell
                        if (exit.IsClosed)
                            message.Append(" (CLOSED)");
                        if (exit.IsHidden)
                            message.Append(" [HIDDEN]");
                        if (ImpersonatedBy != null && ImpersonatedBy is IAdmin)
                            message.Append($" [{exit.Destination.Blueprint?.Id.ToString() ?? "???"}]");
                        message.AppendLine();
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

        private bool FindItemByExtraDescriptionOrName(CommandParameter parameter, out string description) // Find by extra description then name (search in inventory, then equipment, then in room)
        {
            description = null;
            int count = 0;
            foreach (IItem item in Content.Where(CanSee)
                .Concat(Equipments.Where(x => x.Item != null && CanSee(x.Item)).Select(x => x.Item))
                .Concat(Room.Content.Where(CanSee)))
            {
                // Search in item extra description keywords
                if (item.ExtraDescriptions != null)
                {
                    foreach (KeyValuePair<string, string> extraDescription in item.ExtraDescriptions)
                        if (parameter.Tokens.All(x => FindHelpers.StringStartsWith(extraDescription.Key, x))
                            && ++count == parameter.Count)
                        {
                            description = extraDescription.Value;
                            return true;
                        }
                }
                // Search in item keywords
                if (FindHelpers.StringListStartsWith(item.Keywords, parameter.Tokens)
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
                peopleInRoom.AppendFormatLine(" - {0}", victim.DisplayName); // TODO: use RelativeDisplayName ???
            return peopleInRoom;
        }

        private string FormatItem(IItem item, bool shortDisplay) // TODO: (see act_info.C:170 format_obj_to_char)
        {
            return shortDisplay
                ? item.RelativeDisplayName(this) //item.DisplayName
                : item.RelativeDescription(this);
        }

        private static string EquipmentSlotsToString(EquipmentSlots slot)
        {
            switch (slot)
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
                case EquipmentSlots.RingLeft:
                    return "%C%<worn on left finger>    %x%";
                case EquipmentSlots.RingRight:
                    return "%C%<worn on right finger>   %x%";
                case EquipmentSlots.Legs:
                    return "%C%<worn on legs>           %x%";
                case EquipmentSlots.Feet:
                    return "%C%<worn on feet>           %x%";
                case EquipmentSlots.Trinket1:
                    return "%C%<worn as 1st trinket>    %x%";
                case EquipmentSlots.Trinket2:
                    return "%C%<worn as 2nd trinket>    %x%";
                case EquipmentSlots.Wield:
                    return "%C%<wielded>                %x%";
                case EquipmentSlots.Wield2:
                    return "%c%<offhand>                %x%";
                case EquipmentSlots.Hold:
                    return "%C%<held>                   %x%";
                case EquipmentSlots.Shield:
                    return "%C%<worn as shield>         %x%";
                case EquipmentSlots.Wield2H:
                    return "%C%<wielded 2-handed>       %x%";
                case EquipmentSlots.Wield3:
                    return "%c%<3rd wield>              %x%";
                case EquipmentSlots.Wield4:
                    return "%c%<4th wield>              %x%";
                case EquipmentSlots.Wield2H2:
                    return "%C%<wielded 2nd 2-handed>   %x%";
                default:
                    Log.Default.WriteLine(LogLevels.Error, "DoEquipment: missing WearLocation {0}", slot);
                    break;
            }
            return "%C%<unknown>%x%";
        }
    }
}
