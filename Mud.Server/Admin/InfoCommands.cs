using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.DataStructures.HeapPriorityQueue;
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
            foreach (IPlayer player in Repository.Server.GetPlayers())
            {
                switch (player.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        if (player.Impersonating != null)
                            sb.AppendFormatLine("[ IG] {0} playing {1} [lvl: {2} Class: {3} Race: {4}]",
                                player.DisplayName,
                                player.Impersonating.DisplayName,
                                player.Impersonating.Level,
                                player.Impersonating.Class == null ? "(none)" : player.Impersonating.Class.DisplayName,
                                player.Impersonating.Race == null ? "(none)" : player.Impersonating.Race.DisplayName);
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
            foreach (IAdmin admin in Repository.Server.GetAdmins())
            {
                switch (admin.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        if (admin.Impersonating != null)
                            sb.AppendFormatLine("[ IG] {0} impersonating {1}", admin.DisplayName, admin.Impersonating.Name);
                        else if (admin.Incarnating != null)
                            sb.AppendFormatLine("[ IG] {0} incarnating {1}", admin.DisplayName, admin.Incarnating.Name);
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
            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine("+-------------------------------------------+");
            //sb.AppendLine("| Abilities                                 |");
            //sb.AppendLine("+-----------------------+----------+--------+");
            //sb.AppendLine("| Name                  | Resource | Cost   |");
            //sb.AppendLine("+-----------------------+----------+--------+");
            //List<IAbility> abilities = Repository.AbilityManager.Abilities
            //    .Where(x => (x.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed)
            //    .OrderBy(x => x.Name)
            //    .ToList();
            //foreach (IAbility ability in abilities)
            //{
            //    if ((ability.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
            //        sb.AppendFormatLine("| {0,21} |  passive ability  |", ability.Name, StringHelpers.ResourceColor(ability.ResourceKind), ability.CostAmount);
            //    else if (ability.CostType == AmountOperators.Percentage)
            //        sb.AppendFormatLine("| {0,21} | {1,14} | {2,5}% |", ability.Name, StringHelpers.ResourceColor(ability.ResourceKind), ability.CostAmount);
            //    else if (ability.CostType == AmountOperators.Fixed)
            //        sb.AppendFormatLine("| {0,21} | {1,14} | {2,6} |", ability.Name, StringHelpers.ResourceColor(ability.ResourceKind), ability.CostAmount);
            //    else
            //        sb.AppendFormatLine("| {0,21} | free cost ability |", ability.Name, ability.ResourceKind, ability.CostAmount, ability.CostType == AmountOperators.Percentage ? "%" : " ");
            //}
            //sb.AppendLine("+-----------------------+----------+--------+");
            //Page(sb);

            List<IAbility> abilities = Repository.AbilityManager.Abilities
                .Where(x => (x.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed)
                .OrderBy(x => x.Name)
                .ToList();
            StringBuilder sb = AbilitiesTableGenerator.Value.Generate(abilities);
            Page(sb);
            return true;
        }

        [Command("mstat", Category = "Information")]
        protected virtual bool DoMstat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("mstat whom?" + Environment.NewLine);
            else
            {
                ICharacter victim = FindHelpers.FindByName(Repository.World.Characters, parameters[0]);
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
                    sb.AppendFormatLine("Name: {0}", victim.DisplayName);
                    sb.AppendFormatLine("DisplayName: {0}", victim.DisplayName);
                    if (victim.Leader != null)
                        sb.AppendFormatLine("Leader: {0}", victim.Leader.DisplayName);
                    if (victim.GroupMembers.Any())
                        foreach (ICharacter member in victim.GroupMembers)
                            sb.AppendFormatLine("Group member: {0}", member.DisplayName);
                    if (victim.Slave != null)
                        sb.AppendFormatLine("Slave: {0}", victim.Slave.DisplayName);
                    if (victim.ImpersonatedBy != null)
                        sb.AppendFormatLine("Impersonated by {0}", victim.ImpersonatedBy.DisplayName);
                    else
                        sb.AppendFormatLine("Impersonable: {0}", victim.Impersonable);
                    if (victim.ControlledBy != null)
                        sb.AppendFormatLine("Controlled by {0}", victim.ControlledBy.DisplayName);
                    if (victim.Fighting != null)
                        sb.AppendFormatLine("Fighting: {0}", victim.Fighting.DisplayName);
                    sb.AppendFormatLine("Room: {0} [vnum: {1}]", victim.Room.DisplayName, victim.Room.Blueprint?.Id ?? -1);
                    sb.AppendFormatLine("Race: {0} Class: {1}", victim.Race == null ? "(none)" : victim.Race.DisplayName, victim.Class == null ? "(none)" : victim.Class.DisplayName);
                    sb.AppendFormatLine("Level: {0} Sex: {1}", victim.Level, victim.Sex);
                    sb.AppendFormatLine("Hitpoints: Current: {0} Max: {1}", victim.HitPoints, victim[ComputedAttributeTypes.MaxHitPoints]);
                    sb.AppendLine("Attributes:");
                    foreach (PrimaryAttributeTypes primaryAttribute in EnumHelpers.GetValues<PrimaryAttributeTypes>())
                        sb.AppendFormatLine("{0}: Current: {1} Base: {2}", primaryAttribute, victim[primaryAttribute], victim.GetBasePrimaryAttribute(primaryAttribute));
                    foreach (ComputedAttributeTypes computedAttribute in EnumHelpers.GetValues<ComputedAttributeTypes>())
                        sb.AppendFormatLine("{0}: {1}", computedAttribute, victim[computedAttribute]);
                    foreach (ResourceKinds resourceKind in EnumHelpers.GetValues<ResourceKinds>().Where(x => x != ResourceKinds.None))
                        sb.AppendFormatLine("{0}: {1}", resourceKind, victim[resourceKind]);
                    foreach (IPeriodicAura pa in victim.PeriodicAuras)
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

        [Command("ostat", Category = "Information")]
        protected virtual bool DoOstat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("ostat what?"+Environment.NewLine);
            else
            {
                IItem item = FindHelpers.FindByName(Repository.World.Items, parameters[0]);
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
#pragma warning disable 642
                                ; // TODO: additional info for IItemCorpse
#pragma warning restore 642
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

        [Command("path", Category = "Information")]
        protected virtual bool DoPath(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating == null)
            {
                Send("Map can only be used when impersonating." + Environment.NewLine);
                return true;
            }

            if (parameters.Length == 0)
            {
                Send("Path to where ?" + Environment.NewLine);
                return true;
            }

            // TODO: destination to mob or room id
            //IRoom destination = Impersonating.Room.GetRoom(ExitDirections.South).GetRoom(ExitDirections.South).GetRoom(ExitDirections.South).GetRoom(ExitDirections.East).GetRoom(ExitDirections.East).GetRoom(ExitDirections.South).GetRoom(ExitDirections.South).GetRoom(ExitDirections.West).GetRoom(ExitDirections.West);
            IRoom destination = FindHelpers.FindByName(Repository.World.Rooms, parameters[0]);

            if (destination == null)
            {
                Send("Destination not found." + Environment.NewLine);
                return true;
            }

            string path = BuildPath(Impersonating.Room, destination);

            Send("Following path will lead to {0}:" + Environment.NewLine + "%c%" + path + "%x%" + Environment.NewLine, destination.DisplayName);

            return true;
        }

        //*********************** Helpers ***************************

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
                return "Path not found.";
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
