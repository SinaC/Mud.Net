using System;
using System.Linq;
using System.Text;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        // TODO: display groups
        [Command("mstat")]
        protected virtual bool DoMstat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("mstat whom?");
            else
            {
                ICharacter victim = FindHelpers.FindByName(Repository.World.GetCharacters(), parameters[0]);
                if (victim == null)
                    Send(StringHelpers.NotFound);
                else
                {
                    StringBuilder sb = new StringBuilder();
                    if (victim.Blueprint != null)
                        sb.AppendFormatLine("Blueprint: {0}", victim.Blueprint.Id);
                        // TODO: display blueprint
                    else
                        sb.AppendLine("No blueprint");
                    sb.AppendFormatLine("Name: {0}", victim.Name);
                    sb.AppendFormatLine("DisplayName: {0}", victim.DisplayName);
                    if (victim.Leader != null)
                        sb.AppendFormatLine("Leader: {0}", victim.Leader.Name);
                    if (victim.GroupMembers.Any())
                        foreach (ICharacter member in victim.GroupMembers)
                            sb.AppendFormatLine("Group member: {0}", member.Name);
                    if (victim.Slave != null)
                        sb.AppendFormatLine("Slave: {0}", victim.Slave.Name);
                    if (victim.ImpersonatedBy != null)
                        sb.AppendFormatLine("Impersonated by {0}", victim.ImpersonatedBy.Name);
                    if (victim.ControlledBy != null)
                        sb.AppendFormatLine("Controlled by {0}", victim.ControlledBy.Name);
                    if (victim.Fighting != null)
                        sb.AppendFormatLine("Fighting: {0}", victim.Fighting.Name);
                    sb.AppendFormatLine("Room: {0} [vnum: {1}]", victim.Room.Name, victim.Room.Blueprint == null ? -1 : victim.Room.Blueprint.Id);
                    sb.AppendFormatLine("Level: {0} Sex: {1}", victim.Level, victim.Sex);
                    sb.AppendFormatLine("Hitpoints: Current: {0} Max: {1}", victim.HitPoints, victim[ComputedAttributeTypes.MaxHitPoints]);
                    sb.AppendLine("Attributes:");
                    foreach (PrimaryAttributeTypes primaryAttribute in EnumHelpers.GetValues<PrimaryAttributeTypes>())
                        sb.AppendFormatLine("{0}: Current: {1} Base: {2}", primaryAttribute, victim[primaryAttribute], victim.GetBasePrimaryAttribute(primaryAttribute));
                    foreach (ComputedAttributeTypes computedAttribute in EnumHelpers.GetValues<ComputedAttributeTypes>())
                        sb.AppendFormatLine("{0}: {1}", computedAttribute, victim[computedAttribute]);
                    foreach(IPeriodicAura pa in victim.PeriodicAuras)
                        if (pa.AuraType == PeriodicAuraTypes.Damage) // TODO: operator
                            sb.AppendFormatLine("{0} from {1}: {2} {3}{4} damage every {5} seconds for {6} seconds.",
                                pa.Ability == null ? "(none)" : pa.Ability.Name,
                                pa.Source == null ? "(none)" : pa.Source.DisplayName,
                                pa.Amount,
                                pa.AmountOperator == AmountOperators.Fixed ? String.Empty : "%",
                                pa.School,
                                pa.TickDelay,
                                pa.SecondsLeft);
                        else
                            sb.AppendFormatLine("{0} from {1}: {2}{3} heal every {4} seconds for {5} seconds.",
                                pa.Ability == null ? "(none)" : pa.Ability.Name,
                                pa.Source == null ? "(none)" : pa.Source.DisplayName,
                                pa.Amount,
                                pa.AmountOperator == AmountOperators.Fixed ? String.Empty : "%",
                                pa.TickDelay,
                                pa.SecondsLeft);
                    foreach (IAura aura in victim.Auras)
                        sb.AppendFormatLine("{0} from {1} modifies {2} by {3}{4} for {5} seconds.",
                            aura.Ability == null ? "(none)" : aura.Ability.Name,
                            aura.Source == null ? "(none)" : aura.Source.DisplayName,
                            aura.Modifier,
                            aura.Amount,
                            aura.AmountOperator == AmountOperators.Fixed ? String.Empty : "%",
                            aura.SecondsLeft);
                    Send(sb);
                }
            }
            return true;
        }

        [Command("ostat")]
        protected virtual bool DoOstat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("ostat what?");
            else
            {
                IItem item = FindHelpers.FindByName(Repository.World.GetItems(), parameters[0]);
                if (item == null)
                    Send(StringHelpers.NotFound);
                else
                {
                    StringBuilder sb = new StringBuilder();
                    if (item.Blueprint != null)
                        sb.AppendFormatLine("Blueprint: {0}", item.Blueprint.Id);
                    // TODO: display blueprint
                    else
                        sb.AppendLine("No blueprint");
                    sb.AppendFormatLine("Name: {0}", item.Name);
                    sb.AppendFormatLine("DisplayName: {0}", item.DisplayName);
                    if (item.ContainedInto != null)
                        sb.AppendFormatLine("Contained in {0}", item.ContainedInto.Name);
                    IEquipable equipable = item as IEquipable;
                    if (equipable != null)
                        sb.AppendFormatLine("Equiped by {0}", equipable.EquipedBy == null ? "(none)" : equipable.EquipedBy.Name);
                    else
                        sb.AppendLine("Cannot be equiped");
                    sb.AppendFormatLine("Cost: {0} Weight: {1}", item.Cost, item.Weight);
                    sb.AppendFormatLine("Type: {0}", item.GetType().Name);
                    IItemArmor armor = item as IItemArmor;
                    if (armor != null)
                        sb.AppendFormatLine("Armor type: {0} Armor value: {1}", armor.ArmorKind, armor.Armor);
                    else
                    {
                        IItemContainer container = item as IItemContainer;
                        if (container != null)
                            sb.AppendFormatLine("Item count: {0} Weight multiplier: {1}", container.ItemCount, container.WeightMultiplier);
                        else
                        {
                            IItemCorpse corpse = item as IItemCorpse;
                            if (corpse != null)
                                ; // TODO: additional info for IItemCorpse
                            else
                            {
                                IItemLight light = item as IItemLight;
                                if (light != null)
                                    sb.AppendFormatLine("Time left: {0}", light.TimeLeft);
                                else
                                {
                                    IItemWeapon weapon = item as IItemWeapon;
                                    if (weapon != null)
                                        sb.AppendFormatLine("Weapon type: {0}  {1}d{2} {3}", weapon.Type, weapon.DiceCount, weapon.DiceValue, weapon.DamageType);
                                    else
                                        sb.AppendLine("UNHANDLED ITEM TYPE");
                                }
                            }
                        }
                    }
                    Send(sb);
                }
            }
            return true;
        }

        // TODO: redo from scratch
        [Command("map")]
        protected virtual bool DoMap(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating == null)
            {
                Send("Map can only be used when impersonating." + Environment.NewLine);
                return true;
            }

            const int halfSize = 5;
            int[,] map = new int[halfSize * 2 + 1, halfSize * 2 + 1];
            MapArea(map, Impersonating.Room, halfSize, halfSize, 1, halfSize*2-1);
            map[halfSize, halfSize] = 3;

            StringBuilder sb = new StringBuilder();
            const int size = halfSize*2 + 1;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                    sb.Append(map[x, y]);
                sb.AppendLine();
            }
            Send(sb);

            return true;
        }

        //*********************** Helpers ***************************
        // Map values: 0: not visited | 1: empty | 2: one way/maze
        private void MapArea(int[,] map, IRoom room, int x, int y, int min, int max)
        {
            map[x, y] = 1; // mark as visited
            for (int i = 0; i < 4; i++)
            {
                IExit exit = room.Exits[i];
                if (exit != null && exit.Destination != null && x >= min && y >= min && x <= max && y <= max)
                {
                    IRoom prospectRoom = exit.Destination;
                    ExitDirections reverse = ExitHelpers.ReverseDirection((ExitDirections)i);
                    IExit reverseExit = prospectRoom.Exit(reverse);
                    if (reverseExit.Destination != room)
                        map[x, y] = 2; // one way/maze
                    int offsetX = 0;
                    int offsetY = 0;
                    if (i == 0)
                    {
                        offsetX = -1;
                        offsetY = 0;
                    }
                    else if (i == 1)
                    {
                        offsetX = 0;
                        offsetY = 1;
                    }
                    else if (i == 2)
                    {
                        offsetX = 1;
                        offsetY = 0;
                    }
                    else if (i == 3)
                    {
                        offsetX = 0;
                        offsetY = -1;
                    }
                    if (map[x+offsetX,y+offsetY] == 0)
                        MapArea(map, prospectRoom, x+offsetX, y+offsetY, min, max);
                }
            }
        }
    }
}
