using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using Mud.DataStructures.HeapPriorityQueue;
using Mud.Server.Abilities;
using Mud.Server.Common;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server.Admin
{
    // TODO: command to display races, classes
    public partial class Admin
    {
        [Command("who", Category = "Information")]
        protected override bool DoWho(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            //
            sb.AppendFormatLine("Players:");
            foreach (IPlayer player in Repository.Server.Players)
            {
                switch (player.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        if (player.Impersonating != null)
                            sb.AppendFormatLine("[ IG] {0} playing {1} [lvl: {2} Class: {3} Race: {4}]",
                                player.DisplayName,
                                player.Impersonating.DisplayName,
                                player.Impersonating.Level,
                                player.Impersonating.Class?.DisplayName ?? "(none)",
                                player.Impersonating.Race?.DisplayName ?? "(none)");
                        else
                            sb.AppendFormatLine("[ IG] {0} playing something", player.DisplayName);
                        break;
                    default:
                        sb.AppendFormatLine("[OOG] {0}", player.DisplayName);
                        break;
                }
            }
            //
            sb.AppendFormatLine("Admins:");
            foreach (IAdmin admin in Repository.Server.Admins)
            {
                switch (admin.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        if (admin.Impersonating != null)
                            sb.AppendFormatLine("[ IG] {0} impersonating {1}", admin.DisplayName, admin.Impersonating.DisplayName);
                        else if (admin.Incarnating != null)
                            sb.AppendFormatLine("[ IG] {0} incarnating {1}", admin.DisplayName, admin.Incarnating.DisplayName);
                        else
                            sb.AppendFormatLine("[ IG] {0} neither playing nor incarnating !!!", admin.DisplayName);
                        break;
                    default:
                        sb.AppendFormatLine("[OOG] {0} {1}", admin.DisplayName, admin.PlayerState);
                        break;
                }
            }
            //
            Page(sb);
            return true;
        }

        [Command("spells", Category = "Information")]
        [Command("skills", Category = "Information")]
        [Command("abilities", Category = "Information")]
        protected virtual bool DoAbilities(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: 1st parameter is class or race
            // TODO: color
            // TODO: split into spells/skills
            //+------------------------------------------------------+
            //| Abilities                                            |
            //+-----------------------+----------+--------+----------+
            //| Name                  | Resource | Cost   | Cooldown |
            //+-----------------------+----------+--------+----------+ 

            List<IAbility> abilities = Repository.AbilityManager.Abilities
                .Where(x => (x.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed)
                .OrderBy(x => x.Name)
                .ToList();
            StringBuilder sb = AbilitiesTableGenerator.Value.Generate(abilities);
            Page(sb);
            return true;
        }

        [Command("stat", Category = "Information")]
        protected virtual bool DoStat(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormatLine("#Admins: {0}", Repository.Server.Admins.Count());
            sb.AppendFormatLine("#Players: {0}", Repository.Server.Players.Count());
            sb.AppendFormatLine("#Areas: {0}", Repository.World.Areas.Count());
            sb.AppendLine("Blueprints:");
            sb.AppendFormatLine("   #Rooms: {0}", Repository.World.RoomBlueprints.Count);
            sb.AppendFormatLine("   #Characters: {0}", Repository.World.CharacterBlueprints.Count);
            sb.AppendFormatLine("   #Items: {0}", Repository.World.ItemBlueprints.Count);
            sb.AppendLine("Entities:");
            sb.AppendFormatLine("   #Rooms: {0}", Repository.World.Rooms.Count());
            sb.AppendFormatLine("   #Characters: {0}", Repository.World.Characters.Count());
            sb.AppendFormatLine("   #Items: {0}", Repository.World.Items.Count());
            Send(sb);
            return true;
        }

        [Command("rstat", Category = "Information")]
        protected virtual bool DoRstat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Rstat what?");
                return true;
            }
            if (!parameters[0].IsNumber)
            {
                Send("Syntax: rstat <room id>");
                return true;
            }
            int id = parameters[0].AsInt;
            IRoom room = Repository.World.Rooms.FirstOrDefault(x => x.Blueprint.Id == id);
            if (room == null)
            {
                Send("It doesn't exist.");
                return true;
            }
            StringBuilder sb = new StringBuilder();
            if (room.Blueprint != null)
                sb.AppendFormatLine("Blueprint: {0}", room.Blueprint.Id);
            else
                sb.AppendLine("No blueprint");
            sb.AppendFormatLine("Name: {0}", room.Blueprint?.Name ?? "(none)");
            sb.AppendFormatLine("DisplayName: {0}", room.DisplayName);
            sb.AppendFormatLine("Description: {0}", room.Description);
            if (room.ExtraDescriptions != null)
            {
                foreach (KeyValuePair<string, string> kv in room.ExtraDescriptions)
                    sb.AppendFormatLine("ExtraDescription: {0} " + Environment.NewLine + "{1}", kv.Key, kv.Value);
            }
            // TODO: exits
            // TODO: content
            // TODO: people
            Send(sb);
            return true;
        }

        [Command("cstat", Category = "Information")]
        protected virtual bool DoCstat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Cstat whom?");
            else
            {
                ICharacter victim = Impersonating == null 
                    ? FindHelpers.FindByName(Repository.World.Characters, parameters[0])
                    : FindHelpers.FindChararacterInWorld(Impersonating, parameters[0]);
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
                    sb.AppendFormatLine("Description: {0}", victim.Description);
                    if (victim.Leader != null)
                        sb.AppendFormatLine("Leader: {0}", victim.Leader.DisplayName);
                    if (victim.GroupMembers.Any())
                        foreach (ICharacter member in victim.GroupMembers)
                            sb.AppendFormatLine("Group member: {0}", member.DisplayName);
                    if (victim.Slave != null)
                        sb.AppendFormatLine("Slave: {0}", victim.Slave.DisplayName);
                    if (victim.IncarnatedBy != null)
                        sb.AppendFormatLine("Incarnated by {0}", victim.IncarnatedBy.DisplayName);
                    else
                        sb.AppendFormatLine("Incarnatable: {0}", victim.Incarnatable);
                    if (victim.ImpersonatedBy != null)
                        sb.AppendFormatLine("Impersonated by {0}", victim.ImpersonatedBy.DisplayName);
                    else
                        sb.AppendFormatLine("Impersonable: {0}", victim.Impersonable);
                    if (victim.ControlledBy != null)
                        sb.AppendFormatLine("Controlled by {0}", victim.ControlledBy.DisplayName);
                    if (victim.Fighting != null)
                        sb.AppendFormatLine("Fighting: {0}", victim.Fighting.DisplayName);
                    sb.AppendFormatLine("Position: {0}", victim.Position);
                    sb.AppendFormatLine("Furniture: {0}", victim.Furniture?.DisplayName ?? "(none)");
                    sb.AppendFormatLine("Room: {0} [vnum: {1}]", victim.Room.DisplayName, victim.Room.Blueprint?.Id ?? -1);
                    sb.AppendFormatLine("Race: {0} Class: {1}", victim.Race?.DisplayName ?? "(none)", victim.Class?.DisplayName ?? "(none)");
                    sb.AppendFormatLine("Level: {0} Sex: {1}", victim.Level, victim.Sex);
                    sb.AppendFormatLine("Experience: {0} NextLevel: {1}", victim.Experience, victim.ExperienceToLevel);
                    sb.AppendFormatLine("Hitpoints: Current: {0} Max: {1}", victim.HitPoints, victim[SecondaryAttributeTypes.MaxHitPoints]);
                    sb.AppendLine("Attributes:");
                    foreach (PrimaryAttributeTypes primaryAttribute in EnumHelpers.GetValues<PrimaryAttributeTypes>())
                        sb.AppendFormatLine("{0}: Current: {1} Base: {2}", primaryAttribute, victim[primaryAttribute], victim.GetBasePrimaryAttribute(primaryAttribute));
                    foreach (SecondaryAttributeTypes secondary in EnumHelpers.GetValues<SecondaryAttributeTypes>())
                        sb.AppendFormatLine("{0}: {1}", secondary, victim[secondary]);
                    foreach (ResourceKinds resourceKind in EnumHelpers.GetValues<ResourceKinds>().Where(x => x != ResourceKinds.None))
                        sb.AppendFormatLine("{0}: {1}", resourceKind, victim[resourceKind]);
                    foreach (IPeriodicAura pa in victim.PeriodicAuras)
                        if (pa.AuraType == PeriodicAuraTypes.Damage) // TODO: operator
                            sb.AppendFormatLine("{0} from {1}: {2} {3}{4} damage every {5} seconds for {6} seconds.",
                                pa.Ability?.Name ?? "(none)",
                                pa.Source?.DisplayName ?? "(none)",
                                pa.Amount,
                                pa.AmountOperator == AmountOperators.Fixed ? String.Empty : "%",
                                pa.School,
                                pa.TickDelay,
                                pa.SecondsLeft);
                        else
                            sb.AppendFormatLine("{0} from {1}: {2}{3} heal every {4} seconds for {5} seconds.",
                                pa.Ability?.Name ?? "(none)",
                                pa.Source?.DisplayName ?? "(none)",
                                pa.Amount,
                                pa.AmountOperator == AmountOperators.Fixed ? String.Empty : "%",
                                pa.TickDelay,
                                pa.SecondsLeft);
                    foreach (IAura aura in victim.Auras)
                        sb.AppendFormatLine("{0} from {1} modifies {2} by {3}{4} for {5} seconds.",
                            aura.Ability?.Name ?? "(none)",
                            aura.Source?.DisplayName ?? "(none)",
                            aura.Modifier,
                            aura.Amount,
                            aura.AmountOperator == AmountOperators.Fixed ? String.Empty : "%",
                            aura.SecondsLeft);
                    if (victim.KnownAbilities.Any())
                    {
                        sb.AppendLine("Abilities:");
                        foreach (AbilityAndLevel abilityAndLevel in victim.KnownAbilities.Where(x => (x.Ability.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed).OrderBy(x => x.Level).ThenBy(x => x.Ability.Name))
                            sb.AppendFormatLine("{0} - {1}[{2}]", abilityAndLevel.Level, abilityAndLevel.Ability.Name, abilityAndLevel.Ability.Id);
                    }
                    else
                        sb.AppendLine("No abilities");
                    Send(sb);
                }
            }
            return true;
        }

        [Command("ostat", Category = "Information")]
        protected virtual bool DoOstat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Ostat what?");
            else
            {
                IItem item = Impersonating == null
                    ? FindHelpers.FindByName(Repository.World.Items, parameters[0])
                    : FindHelpers.FindItemHere(Impersonating, parameters[0]);
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
                    sb.AppendFormatLine("Description: {0}", item.Description);
                    if (item.ExtraDescriptions != null)
                    {
                        foreach (KeyValuePair<string, string> kv in item.ExtraDescriptions)
                            sb.AppendFormatLine("ExtraDescription: {0} " + Environment.NewLine + "{1}", kv.Key, kv.Value);
                    }
                    sb.AppendFormatLine("Type: {0}", item.GetType().Name);
                    if (item.IncarnatedBy != null)
                        sb.AppendFormatLine("Incarnated by {0}", item.IncarnatedBy.DisplayName);
                    else
                        sb.AppendFormatLine("Incarnatable: {0}", item.Incarnatable);
                    if (item.ContainedInto != null)
                        sb.AppendFormatLine("Contained in {0}", item.ContainedInto.DisplayName);
                    IEquipable equipable = item as IEquipable;
                    if (equipable != null)
                        sb.AppendFormatLine("Equiped by {0} on {1}", equipable.EquipedBy?.DisplayName ?? "(none)", equipable.WearLocation);
                    else
                        sb.AppendLine("Cannot be equiped");
                    sb.AppendFormatLine("Cost: {0} Weight: {1}", item.Cost, item.Weight);
                    IItemArmor armor = item as IItemArmor;
                    if (armor != null)
                        sb.AppendFormatLine("Armor type: {0} Armor value: {1}", armor.ArmorKind, armor.Armor);
                    IItemContainer container = item as IItemContainer;
                    if (container != null)
                        sb.AppendFormatLine("Item count: {0} Weight multiplier: {1}", container.ItemCount, container.WeightMultiplier);
                    //
                    IItemCorpse corpse = item as IItemCorpse;
                    if (corpse != null)
                        sb.AppendLine("No additional informations");
                    //
                    IItemLight light = item as IItemLight;
                    if (light != null)
                        sb.AppendFormatLine("Time left: {0}", light.TimeLeft);
                    //
                    IItemWeapon weapon = item as IItemWeapon;
                    if (weapon != null)
                        sb.AppendFormatLine("Weapon type: {0}  {1}d{2} {3}", weapon.Type, weapon.DiceCount, weapon.DiceValue, weapon.DamageType);
                    //
                    IItemFurniture furniture = item as IItemFurniture;
                    if (furniture != null)
                    {
                        sb.AppendFormatLine("Actions: {0} Preposition: {1} MaxPeople: {2} HealBonus: {3} ResourceBonus: {4}", furniture.FurnitureActions, furniture.FurniturePlacePreposition, furniture.MaxPeople, furniture.HealBonus, furniture.ResourceBonus);
                        List<ICharacter> people = furniture.People.ToList();
                        if (people.Count == 0)
                            sb.AppendLine("None is using it");
                        else
                        {
                            sb.Append("People using it: ");
                            sb.Append(people.Select(x => x.DisplayName).Aggregate((n, i) => n + "," + i));
                            sb.AppendLine();
                        }
                    }
                    //
                    IItemShield shield = item as IItemShield;
                    if (shield != null)
                        sb.AppendFormatLine("Armor: {0}", shield.Armor);
                    // TODO: other item type
                    //
                    Send(sb);
                }
            }
            return true;
        }

        [Command("cfind", Category = "Information")]
        protected virtual bool DoCfind(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Cfind what?");
                return true;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Searching characters '{parameters[0].Value}'");
            List<ICharacter> characters = FindHelpers.FindAllByName(Repository.World.Characters, parameters[0]).OrderBy(x => x.Blueprint?.Id).ToList();
            if (characters.Count == 0)
                sb.AppendLine("No matches");
            else
            {
                sb.AppendLine("Id         DisplayName                    Room");
                foreach (ICharacter character in characters)
                    sb.AppendLine($"{character.Blueprint?.Id.ToString() ?? "Player",-10} {character.DisplayName,-30} {character.Room?.DebugName ?? "none"}");
            }
            Page(sb);

            return true;
        }

        [Command("ofind", Category = "Information")]
        protected virtual bool DoOfind(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Ofind what?");
                return true;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Searching items '{parameters[0].Value}'");
            List<IItem> items = FindHelpers.FindAllByName(Repository.World.Items, parameters[0]).OrderBy(x => x.Blueprint?.Id).ToList();
            if (items.Count == 0)
                sb.AppendLine("No matches");
            else
            {
                sb.AppendLine("Id         DisplayName                    ContainedInto");
                foreach (IItem item in items)
                    sb.AppendLine($"{item.Blueprint.Id,-10} {item.DisplayName,-30} {DisplayEntityAndContainer(item) ?? "(none)"}");
            }
            Page(sb);

            return true;
        }

        [Command("path", Category = "Information")]
        protected virtual bool DoPath(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating == null)
            {
                Send("Map can only be used when impersonating.");
                return true;
            }

            if (parameters.Length == 0)
            {
                Send("Path to where ?");
                return true;
            }

            IRoom destination = FindHelpers.FindLocation(Impersonating, parameters[0]);
            if (destination == null)
            {
                Send("No such location.");
                return true;
            }

            string path = BuildPath(Impersonating.Room, destination);

            Send("Following path will lead to {0}:" + Environment.NewLine + "%c%" + path + "%x%", destination.DisplayName);

            return true;
        }

        //*********************** Helpers ***************************
        private static string DisplayEntityAndContainer(IEntity entity)
        {
            if (entity == null)
                return "???";
            StringBuilder sb = new StringBuilder();
            sb.Append(entity.DebugName);
            // don't to anything if entity is IRoom
            IItem item = entity as IItem;
            if (item != null)
            {
                if (item.ContainedInto != null)
                {
                    sb.Append(" in ");
                    sb.Append("{");
                    sb.Append(DisplayEntityAndContainer(item.ContainedInto));
                    sb.Append("}");
                }
                else
                {
                    IEquipable equipable = item as IEquipable;
                    if (equipable != null)
                    {
                        sb.Append(" equiped by ");
                        sb.Append("{");
                        sb.Append(DisplayEntityAndContainer(equipable.EquipedBy));
                        sb.Append("}");
                    }
                }
            }
            ICharacter character = entity as ICharacter;
            if (character != null)
            {
                sb.Append("{");
                sb.Append(DisplayEntityAndContainer(character.Room));
                sb.Append("}");
            }
            return sb.ToString();
        }

        private static readonly Lazy<TableGenerator<IAbility>> AbilitiesTableGenerator = new Lazy<TableGenerator<IAbility>>(() => GenerateAbilitiesTableGenerator);

        private static TableGenerator<IAbility> GenerateAbilitiesTableGenerator
        {
            get
            {
                TableGenerator<IAbility> generator = new TableGenerator<IAbility>("Abilities");
                generator.AddColumn("Name", 23, x => x.Name);
                generator.AddColumn("Resource", 10,
                    x =>
                    {
                        if ((x.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
                            return "%m%passive ability%x%";
                        if (x.CostType == AmountOperators.Percentage || x.CostType == AmountOperators.Fixed)
                            return StringHelpers.ResourceColor(x.ResourceKind);
                        return "%W%free cost ability%x%";
                    },
                    x =>
                    {
                        if ((x.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
                            return 1;
                        if (x.CostType == AmountOperators.Percentage || x.CostType == AmountOperators.Fixed)
                            return 0;
                        return 1;
                    });
                generator.AddColumn("Cost", 8, x => x.CostAmount.ToString(), x => x.CostType == AmountOperators.Percentage ? "% " : " ");
                generator.AddColumn("Cooldown", 10, x => x.Cooldown > 0 ? StringHelpers.FormatDelayShort(x.Cooldown) : "---");
                return generator;
            }
        }

        //

        private string BuildPath(IRoom origin, IRoom destination)
        {
            Dictionary<IRoom, int> distance = new Dictionary<IRoom, int>(500);
            Dictionary<IRoom, Tuple<IRoom, ExitDirections>> previousRoom = new Dictionary<IRoom, Tuple<IRoom, ExitDirections>>(500);
            HeapPriorityQueue<IRoom> pQueue = new HeapPriorityQueue<IRoom>(500);

            // Search path
            distance[origin] = 0;
            pQueue.Enqueue(origin, 0);

            // Dijkstra
            while (!pQueue.IsEmpty())
            {
                IRoom nearest = pQueue.Dequeue();
                if (nearest == destination)
                    break;
                foreach (ExitDirections direction in EnumHelpers.GetValues<ExitDirections>())
                {
                    IRoom neighbour = nearest.GetRoom(direction);
                    if (neighbour != null && !distance.ContainsKey(neighbour))
                    {
                        int neighbourDist = distance[nearest] + 1;
                        int bestNeighbourDist;
                        if (!distance.TryGetValue(neighbour, out bestNeighbourDist))
                            bestNeighbourDist = Int32.MaxValue;
                        if (neighbourDist < bestNeighbourDist)
                        {
                            distance[neighbour] = neighbourDist;
                            pQueue.Enqueue(neighbour, neighbourDist);
                            previousRoom[neighbour] = new Tuple<IRoom, ExitDirections>(nearest, direction);
                        }
                    }
                }
            }

            // Build path
            Tuple<IRoom, ExitDirections> previous;
            if (previousRoom.TryGetValue(destination, out previous))
            {
                StringBuilder sb = new StringBuilder(500);
                while (true)
                {
                    sb.Insert(0, StringHelpers.ShortExitDirections(previous.Item2));
                    if (previous.Item1 == origin)
                        break;
                    if (!previousRoom.TryGetValue(previous.Item1, out previous))
                    {
                        sb.Insert(0, "???");
                        break;
                    }
                }
                // compress path:  ssswwwwnn -> 3s4w2n
                return Compress(sb.ToString());
            }
            else
                return destination.DisplayName + " is here.";
        }

        private static string Compress(string str) //http://codereview.stackexchange.com/questions/64929/string-compression-implementation-in-c
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 1, cnt = 1; i <= str.Length; i++, cnt++)
            {
                if (i == str.Length || str[i] != str[i - 1])
                {
                    if (cnt == 1)
                        builder.Append(str[i - 1]);
                    else
                        builder.Append(cnt).Append(str[i - 1]);
                    cnt = 0;
                }
            }
            return builder.ToString();
        }
    }
}
