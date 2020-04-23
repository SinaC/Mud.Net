using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.DataStructures.HeapPriorityQueue;
using Mud.Domain;
using Mud.Server.Abilities;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("who", Category = "Information")]

        protected override CommandExecutionResults DoWho(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            //
            sb.AppendLine("Players:");
            foreach (IPlayer player in PlayerManager.Players.Where(x => !(x is IAdmin))) // only player
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
            foreach (IAdmin admin in AdminManager.Admins)
            {
                switch (admin.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        if (admin.Impersonating != null)
                            sb.AppendFormatLine("[ IG] [{0}] {1} impersonating {2}", admin.Level, admin.DisplayName, admin.Impersonating.DisplayName);
                        else if (admin.Incarnating != null)
                            sb.AppendFormatLine("[ IG] [{0}] {1} incarnating {2}", admin.Level, admin.DisplayName, admin.Incarnating.DisplayName);
                        else
                            sb.AppendFormatLine("[ IG] [{0}] {1} neither playing nor incarnating !!!", admin.Level, admin.DisplayName);
                        break;
                    default:
                        sb.AppendFormatLine("[OOG] [{0}] {1} {2}", admin.Level, admin.DisplayName, admin.PlayerState);
                        break;
                }
            }
            //
            Page(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("abilities", Category = "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <class>",
            "[cmd] <race>")]
        protected virtual CommandExecutionResults DoAbilities(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayed = DisplayAbilitiesList(x => true, parameters);

            if (!displayed)
                return CommandExecutionResults.SyntaxError;
            return CommandExecutionResults.Ok;
        }

        [Command("spells", Category = "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <class>",
            "[cmd] <race>")]
        protected virtual CommandExecutionResults DoSpells(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayed = DisplayAbilitiesList(x => x == AbilityKinds.Spell, parameters);

            if (!displayed)
                return CommandExecutionResults.SyntaxError;
            return CommandExecutionResults.Ok;
        }

        [Command("skills", Category = "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <class>",
            "[cmd] <race>")]
        protected virtual CommandExecutionResults DoSkills(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayed = DisplayAbilitiesList(x => x == AbilityKinds.Spell, parameters);

            if (!displayed)
                return CommandExecutionResults.SyntaxError;
            return CommandExecutionResults.Ok;
        }

        [Command("wiznet", Category = "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] all",
            "[cmd] <field>")]
        protected virtual CommandExecutionResults DoWiznet(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (WiznetFlags loopFlag in EnumHelpers.GetValues<WiznetFlags>().Where(x => x != WiznetFlags.None))
                {
                    bool isOnLoop = (WiznetFlags & loopFlag) == loopFlag;
                    sb.AppendLine($"{loopFlag,-16} : {(isOnLoop ? "ON" : "OFF")}");
                }
                Send(sb);
                return CommandExecutionResults.Ok;
            }
            if (parameters[0].IsAll)
            {
                foreach (WiznetFlags wiznetFlag in EnumHelpers.GetValues<WiznetFlags>().Where(x => x != WiznetFlags.None))
                    WiznetFlags |= wiznetFlag;
                Send("You will now see every wiznet informations.");
                return CommandExecutionResults.Ok;
            }
            WiznetFlags flag;
            if (!EnumHelpers.TryFindByName<WiznetFlags>(parameters[0].Value.ToLowerInvariant(), out flag) || flag == WiznetFlags.None)
            {
                Send("No such option.");
                return CommandExecutionResults.InvalidParameter;
            }
            bool isOn = (WiznetFlags & flag) == flag;
            if (isOn)
            {
                Send($"You'll no longer see {flag} on wiznet.");
                WiznetFlags &= ~flag;
            }
            else
            {
                Send($"You will now see {flag} on wiznet.");
                WiznetFlags |= flag;
            }
            return CommandExecutionResults.Ok;
        }

        [Command("stat", Category = "Information")]
        protected virtual CommandExecutionResults DoStat(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormatLine("#Admins: {0}", AdminManager.Admins.Count());
            sb.AppendFormatLine("#Players: {0}", PlayerManager.Players.Count());
            sb.AppendFormatLine("#Areas: {0}", World.Areas.Count());
            sb.AppendLine("Blueprints:");
            sb.AppendFormatLine("   #Rooms: {0}", World.RoomBlueprints.Count);
            sb.AppendFormatLine("   #Characters: {0}", World.CharacterBlueprints.Count);
            sb.AppendFormatLine("   #Items: {0}", World.ItemBlueprints.Count);
            sb.AppendFormatLine("   #Quests: {0}", World.QuestBlueprints.Count);
            sb.AppendLine("Entities:");
            sb.AppendFormatLine("   #Rooms: {0}", World.Rooms.Count());
            sb.AppendFormatLine("   #Characters: {0}", World.Characters.Count());
            sb.AppendFormatLine("   #NPC: {0}", World.NonPlayableCharacters.Count());
            sb.AppendFormatLine("   #PC: {0}", World.PlayableCharacters.Count());
            sb.AppendFormatLine("   #Items: {0}", World.Items.Count());
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("rstat", Category = "Information")]
        [Syntax("[cmd] <id>")]
        protected virtual CommandExecutionResults DoRstat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0 && Impersonating == null)
                return CommandExecutionResults.SyntaxError;

            if (parameters.Length >= 1 && !parameters[0].IsNumber)
                return CommandExecutionResults.SyntaxError;

            IRoom room;
            if (Impersonating != null)
                room = Impersonating.Room;
            else
            {
                int id = parameters[0].AsNumber;
                room = World.Rooms.FirstOrDefault(x => x.Blueprint.Id == id);
            }
            if (room == null)
            {
                Send("It doesn't exist.");
                return CommandExecutionResults.TargetNotFound;
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
            foreach (ExitDirections direction in EnumHelpers.GetValues<ExitDirections>())
            {
                IExit exit = room.Exit(direction);
                if (exit?.Destination != null)
                {
                    sb.Append(StringExtensions.UpperFirstLetter(direction.ToString()));
                    sb.Append(" - ");
                    sb.Append(exit.Destination.DisplayName);
                    if (exit.IsClosed)
                        sb.Append(" (CLOSED)");
                    if (exit.IsHidden)
                        sb.Append(" [HIDDEN]");
                    if (exit.IsLocked)
                        sb.AppendFormat(" <Locked> {0}", exit.Blueprint.Key);
                    sb.Append($" [{exit.Destination.Blueprint?.Id.ToString() ?? "???"}]");
                    sb.AppendLine();
                }
                // TODO: exits
                // TODO: content
                // TODO: people
            }
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("cstat", Category = "Information")]
        [Command("mstat", Category = "Information")]
        [Syntax("[cmd] <character>")]
        protected virtual CommandExecutionResults DoCstat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            ICharacter victim = Impersonating == null
                ? FindHelpers.FindByName(World.Characters, parameters[0])
                : FindHelpers.FindChararacterInWorld(Impersonating, parameters[0]);
            if (victim == null)
            {
                Send(StringHelpers.NotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            INonPlayableCharacter nonPlayableVictim = victim as INonPlayableCharacter;
            IPlayableCharacter playableVictim = victim as IPlayableCharacter;
            StringBuilder sb = new StringBuilder();
            if (nonPlayableVictim?.Blueprint != null)
            {
                sb.AppendFormatLine("Blueprint: {0}", nonPlayableVictim.Blueprint.Id);
                // TODO: display blueprint
                if (nonPlayableVictim.Blueprint is CharacterQuestorBlueprint characterQuestorBlueprint)
                {
                    sb.AppendLine($"Quest giver: {characterQuestorBlueprint.QuestBlueprints?.Count ?? 0}");
                    foreach (var questBlueprint in characterQuestorBlueprint.QuestBlueprints ?? Enumerable.Empty<QuestBlueprint>())
                    {
                        sb.AppendLine($"  Quest: {questBlueprint.Id}");
                        sb.AppendLine($"    Title: {questBlueprint.Title}");
                        sb.AppendLine($"    Level: {questBlueprint.Level}");
                        sb.AppendLine($"    Description: {questBlueprint.Description}");
                        sb.AppendLine($"    Experience: {questBlueprint.Experience}");
                        sb.AppendLine($"    Gold: {questBlueprint.Gold}");
                        sb.AppendLine($"    ShouldQuestItemBeDestroyed: {questBlueprint.ShouldQuestItemBeDestroyed}");
                        // TODO: display KillLootTable, ItemObjectives, KillObjectives, LocationObjectives
                    }
                }
            }
            else
                sb.AppendLine("No blueprint");
            sb.AppendFormatLine("Name: {0}", victim.Name);
            sb.AppendFormatLine("DisplayName: {0}", victim.DisplayName);
            sb.AppendFormatLine("Description: {0}", victim.Description);
            if (playableVictim?.Leader != null)
                sb.AppendFormatLine("Leader: {0}", playableVictim.Leader.DisplayName);
            if (playableVictim?.GroupMembers.Any() == true)
                foreach (ICharacter member in playableVictim.GroupMembers)
                    sb.AppendFormatLine("Group member: {0}", member.DisplayName);
            if (victim.Slave != null)
                sb.AppendFormatLine("Slave: {0}", victim.Slave.DisplayName);
            if (victim.IncarnatedBy != null)
                sb.AppendFormatLine("Incarnated by {0}", victim.IncarnatedBy.DisplayName);
            else
                sb.AppendFormatLine("Incarnatable: {0}", victim.Incarnatable);
            if (playableVictim?.ImpersonatedBy != null)
                sb.AppendFormatLine("Impersonated by {0}", playableVictim.ImpersonatedBy.DisplayName);
            if (victim.ControlledBy != null)
                sb.AppendFormatLine("Controlled by {0}", victim.ControlledBy.DisplayName);
            if (victim.Fighting != null)
                sb.AppendFormatLine("Fighting: {0}", victim.Fighting.DisplayName);
            sb.AppendFormatLine("Position: {0}", victim.Position);
            sb.AppendFormatLine("Furniture: {0}", victim.Furniture?.DisplayName ?? "(none)");
            sb.AppendFormatLine("Room: {0} [vnum: {1}]", victim.Room.DisplayName, victim.Room.Blueprint?.Id ?? -1);
            sb.AppendFormatLine("Race: {0} Class: {1}", victim.Race?.DisplayName ?? "(none)", victim.Class?.DisplayName ?? "(none)");
            sb.AppendFormatLine("Level: {0} Sex: {1}", victim.Level, victim.Sex);
            if (playableVictim != null)
                sb.AppendFormatLine("Experience: {0} NextLevel: {1}", playableVictim.Experience, playableVictim.ExperienceToLevel);
            sb.AppendFormatLine("Hitpoints: Current: {0} Max: {1}", victim.HitPoints, victim[SecondaryAttributeTypes.MaxHitPoints]);
            sb.AppendLine("Attributes:");
            foreach (PrimaryAttributeTypes primaryAttribute in EnumHelpers.GetValues<PrimaryAttributeTypes>())
                sb.AppendFormatLine("{0}: Current: {1} Base: {2}", primaryAttribute, victim[primaryAttribute], victim.GetBasePrimaryAttribute(primaryAttribute));
            foreach (SecondaryAttributeTypes secondary in EnumHelpers.GetValues<SecondaryAttributeTypes>())
                sb.AppendFormatLine("{0}: {1}", secondary, victim[secondary]);
            foreach (ResourceKinds resourceKind in EnumHelpers.GetValues<ResourceKinds>().Where(x => x != ResourceKinds.None))
                sb.AppendFormatLine("{0}: {1}", resourceKind, victim[resourceKind]);
            foreach (IPeriodicAura pa in victim.PeriodicAuras)
                if (pa.AuraType == PeriodicAuraTypes.Damage)
                    sb.AppendFormatLine("{0} from {1}: {2} {3}{4} damage every {5} seconds for {6} seconds.",
                        pa.Ability?.Name ?? "(none)",
                        pa.Source?.DisplayName ?? "(none)",
                        pa.Amount,
                        pa.AmountOperator == AmountOperators.Fixed ? string.Empty : "%",
                        pa.School,
                        pa.TickDelay,
                        pa.SecondsLeft);
                else
                    sb.AppendFormatLine("{0} from {1}: {2}{3} heal every {4} seconds for {5} seconds.",
                        pa.Ability?.Name ?? "(none)",
                        pa.Source?.DisplayName ?? "(none)",
                        pa.Amount,
                        pa.AmountOperator == AmountOperators.Fixed ? string.Empty : "%",
                        pa.TickDelay,
                        pa.SecondsLeft);
            foreach (IAura aura in victim.Auras)
                sb.AppendFormatLine("{0} from {1} modifies {2} by {3}{4} for {5} seconds.",
                    aura.Ability?.Name ?? "(none)",
                    aura.Source?.DisplayName ?? "(none)",
                    aura.Modifier,
                    aura.Amount,
                    aura.AmountOperator == AmountOperators.Fixed ? string.Empty : "%",
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
            return CommandExecutionResults.Ok;
        }

        [Command("istat", Category = "Information")]
        [Command("ostat", Category = "Information")]
        [Syntax("[cmd] <item>")]
        protected virtual CommandExecutionResults DoIstat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            IItem item = Impersonating == null
                ? FindHelpers.FindByName(World.Items, parameters[0])
                : FindHelpers.FindItemHere(Impersonating, parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.NotFound);
                return CommandExecutionResults.TargetNotFound;
            }

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
                sb.AppendFormatLine("Contained in {0}", item.ContainedInto.DebugName);
            if (item is IEquipable equipable)
                sb.AppendFormatLine("Equiped by {0} on {1}", equipable.EquipedBy?.DebugName ?? "(none)", equipable.WearLocation);
            else
                sb.AppendLine("Cannot be equiped");
            sb.AppendFormatLine("Cost: {0} Weight: {1}", item.Cost, item.Weight);
            if (item is IItemArmor armor)
                sb.AppendFormatLine("Armor type: {0} Armor value: {1}", armor.ArmorKind, armor.Armor);
            if (item is IItemContainer container)
                sb.AppendFormatLine("Item count: {0} Weight multiplier: {1}", container.ItemCount, container.WeightMultiplier);
            //
            if (item is IItemCorpse corpse)
                sb.AppendLine("No additional informations");
            //
            if (item is IItemLight light)
                sb.AppendFormatLine("Time left: {0}", light.TimeLeft);
            //
            if (item is IItemWeapon weapon)
                sb.AppendFormatLine("Weapon type: {0}  {1}d{2} {3}", weapon.Type, weapon.DiceCount, weapon.DiceValue, weapon.DamageType);
            //
            if (item is IItemFurniture furniture)
            {
                sb.AppendFormatLine("Actions: {0} Preposition: {1} MaxPeople: {2} HealBonus: {3} ResourceBonus: {4}", furniture.FurnitureActions, furniture.FurniturePlacePreposition, furniture.MaxPeople, furniture.HealBonus, furniture.ResourceBonus);
                List<ICharacter> people = furniture.People.ToList();
                if (people.Count == 0)
                    sb.AppendLine("None is using it");
                else
                {
                    sb.Append("People using it: ");
                    sb.Append(people.Select(x => x.DebugName).Aggregate((n, i) => n + "," + i));
                    sb.AppendLine();
                }
            }
            //
            if (item is IItemShield shield)
                sb.AppendFormatLine("Armor: {0}", shield.Armor);
            //
            if (item is IItemPortal portal)
                sb.AppendFormatLine("Destination: {0}", portal.Destination?.DebugName ?? "???");
            // TODO: other item type
            //
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("cfind", Category = "Information")]
        [Command("mfind", Category = "Information")]
        [Syntax("[cmd] <character>")]
        protected virtual CommandExecutionResults DoCfind(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Searching characters '{parameters[0].Value}'");
            List<INonPlayableCharacter> characters = FindHelpers.FindAllByName(World.NonPlayableCharacters, parameters[0]).OrderBy(x => x.Blueprint?.Id).ToList();
            if (characters.Count == 0)
                sb.AppendLine("No matches");
            else
            {
                sb.AppendLine("Id         DisplayName                    Room");
                foreach (INonPlayableCharacter character in characters)
                    sb.AppendLine($"{character.Blueprint?.Id.ToString() ?? "Player",-10} {character.DisplayName,-30} {character.Room?.DebugName ?? "none"}");
            }
            Page(sb);

            return CommandExecutionResults.Ok;
        }

        [Command("ifind", Category = "Information")]
        [Command("ofind", Category = "Information")]
        [Syntax("[cmd] <item>")]
        protected virtual CommandExecutionResults DoIfind(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Searching items '{parameters[0].Value}'");
            List<IItem> items = FindHelpers.FindAllByName(World.Items, parameters[0]).OrderBy(x => x.Blueprint?.Id).ToList();
            if (items.Count == 0)
                sb.AppendLine("No matches");
            else
            {
                sb.AppendLine("Id         DisplayName                    ContainedInto");
                foreach (IItem item in items)
                    sb.AppendLine($"{item.Blueprint.Id,-10} {item.DisplayName,-30} {DisplayEntityAndContainer(item) ?? "(none)"}");
            }
            Page(sb);

            return CommandExecutionResults.Ok;
        }

        [AdminCommand("path", Category = "Information", MustBeImpersonated = true)]
        [Syntax("[cmd] <location>")]
        protected virtual CommandExecutionResults DoPath(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            IRoom destination = FindHelpers.FindLocation(Impersonating, parameters[0]);
            if (destination == null)
            {
                Send("No such location.");
                return CommandExecutionResults.TargetNotFound;
            }

            string path = BuildPath(Impersonating.Room, destination);
            Send("Following path will lead to {0}:" + Environment.NewLine + "%c%" + path + "%x%", destination.DisplayName);
            return CommandExecutionResults.Ok;
        }

        [Command("info", Category = "Information")]
        [Syntax(
            "[cmd] race",
            "[cmd] class",
            "[cmd] <race>",
            "[cmd] <class>")]
        protected virtual CommandExecutionResults DoInfo(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            // class
            if (parameters[0].Value == "class")
            {
                // Name, ShortName, DisplayName, ResourceKinds
                StringBuilder sb = TableGenerators.ClassTableGenerator.Value.Generate("Classes", ClassManager.Classes);
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            // race
            if (parameters[0].Value == "race")
            {
                // Name, ShortName, DisplayName
                StringBuilder sb = TableGenerators.RaceTableGenerator.Value.Generate("Races", RaceManager.Races);
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            // class name
            IClass matchingClass = ClassManager.Classes.FirstOrDefault(x => FindHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (matchingClass != null)
            {
                 StringBuilder sb = TableGenerators.FullInfoAbilityAndLevelTableGenerator.Value.GenerateWithPreHeaders(matchingClass.DisplayName, matchingClass.Abilities.OrderBy(x => x.Level).ThenBy(x => x.Ability.Name), new[] {
                    matchingClass.DisplayName,
                    $"ShortName: {matchingClass.ShortName}",
                    $"Resource(s): {string.Join(",", matchingClass.ResourceKinds?.Select(x => x.ToString()))}"});
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            // race name
            IRace matchingRace = RaceManager.Races.FirstOrDefault(x => FindHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (matchingRace != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityAndLevelTableGenerator.Value.GenerateWithPreHeaders(matchingRace.DisplayName, matchingRace.Abilities.OrderBy(x => x.Level).ThenBy(x => x.Ability.Name), new[] {
                    matchingRace.DisplayName,
                    $"ShortName: {matchingRace.ShortName}" });
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            return CommandExecutionResults.SyntaxError;
        }

        //*********************** Helpers ***************************

        private bool DisplayAbilitiesList(Func<AbilityKinds, bool> filterOnAbilityKind, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                // no filter
                StringBuilder sb = TableGenerators.FullInfoAbilityTableGenerator.Value.Generate("Abilities", AbilityManager.Abilities
                    .Where(x => (x.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed)
                    .OrderBy(x => x.Name));
                Page(sb);
                return true;
            }

            // filter on class?
            IClass matchingClass = ClassManager.Classes.FirstOrDefault(x => FindHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (matchingClass != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityAndLevelTableGenerator.Value.GenerateWithPreHeaders($"Abilities for {matchingClass.DisplayName}", matchingClass.Abilities
                    .OrderBy(x => x.Level)
                    .ThenBy(x => x.Ability.Name),
                    new[] { matchingClass.DisplayName });
                Page(sb);
                return true;
            }

            // filter on race?
            IRace matchingRace = RaceManager.Races.FirstOrDefault(x => FindHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (matchingRace != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityAndLevelTableGenerator.Value.GenerateWithPreHeaders($"Abilities for {matchingRace.DisplayName}", matchingRace.Abilities
                    .OrderBy(x => x.Level)
                    .ThenBy(x => x.Ability.Name),
                    new[] { matchingRace.DisplayName });
                Page(sb);
                return true;
            }
            return false;
        }

        private static string DisplayEntityAndContainer(IEntity entity)
        {
            if (entity == null)
                return "???";
            StringBuilder sb = new StringBuilder();
            sb.Append(entity.DebugName);
            // don't to anything if entity is IRoom
            if (entity is IItem item)
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
                    if (item is IEquipable equipable)
                    {
                        sb.Append(" equipped by ");
                        sb.Append("{");
                        sb.Append(DisplayEntityAndContainer(equipable.EquipedBy));
                        sb.Append("}");
                    }
                }
            }

            if (entity is ICharacter character)
            {
                sb.Append("{");
                sb.Append(DisplayEntityAndContainer(character.Room));
                sb.Append("}");
            }
            return sb.ToString();
        }

        //
 
        private string BuildPath(IRoom origin, IRoom destination)
        {
            if (origin == destination)
                return destination.DisplayName + " is here.";

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
                            bestNeighbourDist = int.MaxValue;
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
            return "No path found.";
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
