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
using Mud.Server.Tables;

// ReSharper disable UnusedMember.Global

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("who", "Information")]

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

        [Command("areas", "Information", Priority = 10)]
        protected override CommandExecutionResults DoAreas(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = TableGenerators.FullInfoAreaTableGenerator.Value.Generate("Areas", World.Areas);
            Page(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("abilities", "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <class>",
            "[cmd] <race>")]
        protected virtual CommandExecutionResults DoAbilities(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayed = DisplayAbilitiesList(null, parameters);

            if (!displayed)
                return CommandExecutionResults.SyntaxError;
            return CommandExecutionResults.Ok;
        }

        [Command("spells", "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <class>",
            "[cmd] <race>")]
        protected virtual CommandExecutionResults DoSpells(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayed = DisplayAbilitiesList(AbilityKinds.Spell, parameters);

            if (!displayed)
                return CommandExecutionResults.SyntaxError;
            return CommandExecutionResults.Ok;
        }

        [Command("skills", "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <class>",
            "[cmd] <race>")]
        protected virtual CommandExecutionResults DoSkills(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayed = DisplayAbilitiesList(AbilityKinds.Spell, parameters);

            if (!displayed)
                return CommandExecutionResults.SyntaxError;
            return CommandExecutionResults.Ok;
        }

        [Command("wiznet", "Information")]
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
                    bool isOnLoop = WiznetFlags.HasFlag(loopFlag);
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

        [Command("stat", "Information")]
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

        [Command("rstat", "Information")]
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
            sb.AppendFormatLine("Flags: {0}/{1}", room.RoomFlags, room.BaseRoomFlags);
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
            AppendAuras(sb, room.Auras);
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("cstat", "Information")]
        [Command("mstat", "Information")]
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
                    sb.AppendLine($"Quest giver: {characterQuestorBlueprint.QuestBlueprints?.Length ?? 0}");
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
                foreach (IPlayableCharacter member in playableVictim.GroupMembers)
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
            sb.AppendFormatLine("Level: {0} Sex: {1} (base: {2})", victim.Level, victim.Sex, victim.BaseSex);
            if (playableVictim != null)
                sb.AppendFormatLine("Experience: {0} NextLevel: {1}", playableVictim.Experience, playableVictim.ExperienceToLevel);
            sb.AppendFormatLine("Hitpoints: Current: {0} Max: {1}", victim.HitPoints, victim[CharacterAttributes.MaxHitPoints]);
            sb.AppendFormatLine("Movepoints: Current: {0} Max: {1}", victim.MovePoints, victim[CharacterAttributes.MaxMovePoints]);
            sb.AppendFormatLine("Flags: {0} (base: {1})", victim.CharacterFlags, victim.BaseCharacterFlags);
            sb.AppendFormatLine("Immunites: {0} (base: {1})  Resistances: {2} (base: {3})  Vulnerabilities: {4} (base: {5})", victim.Immunities, victim.BaseImmunities, victim.Resistances, victim.BaseResistances, victim.Vulnerabilities, victim.BaseVulnerabilities);
            sb.AppendFormatLine("Alignment: {0}", victim.Alignment);
            sb.AppendLine("Attributes:");
            foreach (CharacterAttributes attribute in EnumHelpers.GetValues<CharacterAttributes>())
                sb.AppendFormatLine("{0}: {1} (base: {2})", attribute, victim[attribute], victim.BaseAttribute(attribute));
            foreach (ResourceKinds resourceKind in EnumHelpers.GetValues<ResourceKinds>())
                sb.AppendFormatLine("{0}: {1}", resourceKind, victim[resourceKind]);
            if (nonPlayableVictim != null)
            {
                sb.AppendFormatLine("Act: {0}", nonPlayableVictim.ActFlags);
                sb.AppendFormatLine("Offensive: {0}", nonPlayableVictim.OffensiveFlags);
            }
            if (playableVictim != null)
            {
                sb.Append("Conditions: ");
                sb.AppendLine(string.Join(" ", EnumHelpers.GetValues<Conditions>().Select(x => $"{x}: {playableVictim[x]}")));
            }
            //foreach (IPeriodicAura pa in victim.PeriodicAuras)
            //    if (pa.AuraType == PeriodicAuraTypes.Damage)
            //        sb.AppendFormatLine("{0} from {1}: {2} {3}{4} damage every {5} seconds for {6} seconds.",
            //            pa.Ability?.Name ?? "(none)",
            //            pa.Source?.DisplayName ?? "(none)",
            //            pa.Amount,
            //            pa.AmountOperator == CostAmountOperators.Fixed ? string.Empty : "%",
            //            pa.School,
            //            pa.TickDelay,
            //            pa.SecondsLeft);
            //    else
            //        sb.AppendFormatLine("{0} from {1}: {2}{3} heal every {4} seconds for {5} seconds.",
            //            pa.Ability?.Name ?? "(none)",
            //            pa.Source?.DisplayName ?? "(none)",
            //            pa.Amount,
            //            pa.AmountOperator == CostAmountOperators.Fixed ? string.Empty : "%",
            //            pa.TickDelay,
            //            pa.SecondsLeft);
            AppendAuras(sb, victim.Auras);
            //if (victim.KnownAbilities.Any())
            //{
            //    sb.AppendLine("Abilities:");
            //    int col = 0;
            //    foreach (KnownAbility knownAbility in victim.KnownAbilities.OrderBy(x => x.Level).ThenBy(x => x.Ability.Name))
            //    {
            //        // TODO: formating
            //        sb.AppendFormat("{0} - {1} {2}% [{3}]    ", knownAbility.Level, knownAbility.Ability.Name, knownAbility.Learned, knownAbility.Ability.Id);
            //        if (++col % 3 == 0)
            //            sb.AppendLine();
            //    }
            //    if (col % 3 != 0)
            //        sb.AppendLine();
            //}
            //else
            //    sb.AppendLine("No abilities");
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("istat", "Information")]
        [Command("ostat", "Information")]
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
            if (item is IEquipableItem equipable)
                sb.AppendFormatLine("Equiped by {0} on {1}", equipable.EquipedBy?.DebugName ?? "(none)", equipable.WearLocation);
            else
                sb.AppendLine("Cannot be equiped");
            sb.AppendFormatLine("Level: {0}", item.Level);
            sb.AppendFormatLine("Cost: {0} Weight: {1}", item.Cost, item.Weight);
            if (item.DecayPulseLeft > 0)
                sb.AppendFormatLine("Decay in {0}", StringHelpers.FormatDelay(item.DecayPulseLeft / Pulse.PulsePerSeconds));
            sb.AppendFormatLine("Flags: {0} (base: {1})", item.ItemFlags, item.BaseItemFlags);
            if (item is IItemArmor armor)
                sb.AppendFormatLine("Bash: {0} Pierce: {1} Slash: {2} Exotic: {3}", armor.Bash, armor.Pierce, armor.Slash, armor.Exotic);
            if (item is IItemContainer container)
                sb.AppendFormatLine("Item count: {0} Weight multiplier: {1}", container.ItemCount, container.WeightMultiplier);
            //
            if (item is IItemCorpse corpse)
                sb.AppendLine("No additional informations");
            //
            if (item is IItemWeapon weapon)
                sb.AppendFormatLine("Weapon type: {0}  {1}d{2} {3} {4} (base: {5})", weapon.Type, weapon.DiceCount, weapon.DiceValue, weapon.DamageType, weapon.WeaponFlags, weapon.BaseWeaponFlags);
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
                sb.AppendFormatLine("Destination: {0} Flags: {1} Current charges: {2} Max charges: {3}", portal.Destination?.DebugName ?? "???", portal.PortalFlags, portal.CurrentChargeCount, portal.MaxChargeCount);
            //
            if (item is IItemFountain fountain)
            {
                var liquidInfo = TableValues.LiquidInfo(fountain.LiquidName);
                if (liquidInfo != default)
                    sb.AppendFormatLine("Liquid type: {0} color: {1} proof: {2} full: {3} thirst: {4} food: {5} size: {6}", fountain.LiquidName, liquidInfo.color, liquidInfo.proof, liquidInfo.full, liquidInfo.thirst, liquidInfo.food, liquidInfo.servingsize);
                else
                    sb.AppendFormatLine("Liquid type: {0} (not found in liquid table)", fountain.LiquidName);
            }
            //
            if (item is IItemDrinkContainer drinkContainer)
            {
                sb.AppendFormatLine("Max: {0} Current: {1} Poisoned: {2}", drinkContainer.MaxLiquid, drinkContainer.LiquidLeft, drinkContainer.IsPoisoned);
                var liquidInfo = TableValues.LiquidInfo(drinkContainer.LiquidName);
                if (liquidInfo != default)
                    sb.AppendFormatLine("Liquid type: {0} color: {1} proof: {2} full: {3} thirst: {4} food: {5} size: {6}", drinkContainer.LiquidName, liquidInfo.color, liquidInfo.proof, liquidInfo.full, liquidInfo.thirst, liquidInfo.food, liquidInfo.servingsize);
                else
                    sb.AppendFormatLine("Liquid type: {0} (not found in liquid table)", drinkContainer.LiquidName);
            }
            //
            if (item is IItemFood food)
                sb.AppendFormatLine(" Full: {0} hours Hungry: {1} hours Poisonned: {2}", food.FullHours, food.HungerHours, food.IsPoisoned);

            // TODO: other item type
            //
            AppendAuras(sb, item.Auras);
            //
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("cfind", "Information")]
        [Command("mfind", "Information")]
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

        [Command("ifind", "Information")]
        [Command("ofind", "Information")]
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

        [AdminCommand("path", "Information", MustBeImpersonated = true)]
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

        [Command("info", "Information")]
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
            IClass matchingClass = ClassManager.Classes.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (matchingClass != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate(
                    new[]
                    {
                        matchingClass.DisplayName,
                        $"ShortName: {matchingClass.ShortName}",
                        $"Resource(s): {string.Join(",", matchingClass.ResourceKinds?.Select(x => x.ToString()))}"
                    },
                    matchingClass.Abilities.OrderBy(x => x.Level).ThenBy(x => x.Ability.Name));
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            // race name
            IRace matchingRace = RaceManager.Races.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (matchingRace != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate(
                    new[]
                    {
                        matchingRace.DisplayName,
                        $"ShortName: {matchingRace.ShortName}"
                    },
                    matchingRace.Abilities.OrderBy(x => x.Level).ThenBy(x => x.Ability.Name));
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            return CommandExecutionResults.SyntaxError;
        }

        //*********************** Helpers ***************************

        private bool DisplayAbilitiesList(AbilityKinds? filterOnAbilityKind, params CommandParameter[] parameters)
        {
            string title;
            if (filterOnAbilityKind.HasValue)
            {
                switch (filterOnAbilityKind.Value)
                {
                    case AbilityKinds.Passive: title = "Passives"; break;
                    case AbilityKinds.Spell: title = "Spells"; break;
                    case AbilityKinds.Skill: title = "Skills"; break;
                    default: title = "???"; break;
                }
            }
            else
                title = "Abilities";
            if (parameters.Length == 0)
            {
                // no filter
                StringBuilder sb = TableGenerators.FullInfoAbilityTableGenerator.Value.Generate(title, AbilityManager.Abilities
                    .Where(x => !x.AbilityFlags.HasFlag(AbilityFlags.CannotBeUsed))
                    .OrderBy(x => x.Name));
                Page(sb);
                return true;
            }

            // filter on class?
            IClass matchingClass = ClassManager.Classes.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (matchingClass != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate($"{title} for {matchingClass.DisplayName}", matchingClass.Abilities
                    .OrderBy(x => x.Level)
                    .ThenBy(x => x.Ability.Name));
                Page(sb);
                return true;
            }

            // filter on race?
            IRace matchingRace = RaceManager.Races.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (matchingRace != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate($"{title} for {matchingRace.DisplayName}", matchingRace.Abilities
                    .OrderBy(x => x.Level)
                    .ThenBy(x => x.Ability.Name));
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
                    sb.Append("<");
                    sb.Append(DisplayEntityAndContainer(item.ContainedInto));
                    sb.Append(">");
                }
                else
                {
                    if (item is IEquipableItem equipable)
                    {
                        sb.Append(" equipped by ");
                        sb.Append("<");
                        sb.Append(DisplayEntityAndContainer(equipable.EquipedBy));
                        sb.Append(">");
                    }
                }
            }

            if (entity is ICharacter character)
            {
                sb.Append("<");
                sb.Append(DisplayEntityAndContainer(character.Room));
                sb.Append(">");
            }
            return sb.ToString();
        }

        private void AppendAuras(StringBuilder sb, IEnumerable<IAura> auras)
        {
            foreach (IAura aura in auras)
                aura.Append(sb);
        }

        private string CostAmountOperatorsToString(CostAmountOperators op)
        {
            switch (op)
            {
                case CostAmountOperators.None:
                    return string.Empty;
                case CostAmountOperators.Fixed:
                    return "+/-";
                case CostAmountOperators.Percentage:
                    return "%";
            }
            return "???";
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
