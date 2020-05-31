using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.DataStructures.HeapPriorityQueue;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Reset;
using Mud.Server.Blueprints.Room;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

// ReSharper disable UnusedMember.Global

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [AdminCommand("who", "Information")]

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
                            sb.AppendFormatLine("[ IG] {0} playing {1} [lvl: {2} Class: {3} Race: {4}] {5}",
                                player.DisplayName,
                                player.Impersonating.DisplayName,
                                player.Impersonating.Level,
                                player.Impersonating.Class?.DisplayName ?? "(none)",
                                player.Impersonating.Race?.DisplayName ?? "(none)",
                                player.Impersonating.IsImmortal ? "{IMMORTAL}" : string.Empty);
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
                            sb.AppendFormatLine("[ IG] {0} playing {1} [lvl: {2} Class: {3} Race: {4}] {5}",
                                admin.DisplayName,
                                admin.Impersonating.DisplayName,
                                admin.Impersonating.Level,
                                admin.Impersonating.Class?.DisplayName ?? "(none)",
                                admin.Impersonating.Race?.DisplayName ?? "(none)",
                                admin.Impersonating.IsImmortal ? "{IMMORTAL}" : string.Empty);
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

        [AdminCommand("areas", "Information", Priority = 10)]
        protected override CommandExecutionResults DoAreas(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = TableGenerators.FullInfoAreaTableGenerator.Value.Generate("Areas", World.Areas);
            Page(sb);
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("abilities", "Information")]
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

        [AdminCommand("spells", "Information")]
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

        [AdminCommand("skills", "Information")]
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

        [AdminCommand("wiznet", "Information")]
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
            if (!EnumHelpers.TryFindByName(parameters[0].Value.ToLowerInvariant(), out flag) || flag == WiznetFlags.None)
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

        [AdminCommand("stat", "Information")]
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

        [AdminCommand("rstat", "Information")]
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
            if (room.IsPrivate)
            {
                Send("That room is private right now.");
                return CommandExecutionResults.InvalidTarget;
            }

            StringBuilder sb = new StringBuilder();
            if (room.Blueprint != null)
                sb.AppendFormatLine("Blueprint: {0}", room.Blueprint.Id);
            else
                sb.AppendLine("No blueprint");
            sb.AppendFormatLine("Name: {0} Keywords: {1}", room.Blueprint?.Name ?? "(none)", string.Join(",", room.Keywords));
            sb.AppendFormatLine("DisplayName: {0}", room.DisplayName);
            sb.AppendFormatLine("Description: {0}", room.Description);
            sb.AppendFormatLine("Flags: {0} (base: {1})", room.RoomFlags, room.BaseRoomFlags);
            sb.AppendFormatLine("Light: {0} Sector: {1} MaxSize: {2}", room.Light, room.SectorType, room.MaxSize);
            sb.AppendFormatLine("Heal rate: {0} Resource rate: {1}", room.HealRate, room.ResourceRate);
            if (room.ExtraDescriptions != null)
            {
                foreach (var lookup in room.ExtraDescriptions)
                    foreach (string extraDescr in lookup)
                        sb.AppendFormatLine("ExtraDescription: {0} " + Environment.NewLine + "{1}", lookup.Key, extraDescr);
            }
            foreach (ExitDirections direction in EnumHelpers.GetValues<ExitDirections>())
            {
                IExit exit = room[direction];
                if (exit?.Destination != null)
                {
                    sb.Append(direction.DisplayName());
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

        [AdminCommand("cstat", "Information")]
        [AdminCommand("mstat", "Information")]
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
            sb.AppendFormatLine("Name: {0} Keywords: {1}", victim.Name, string.Join(",", victim.Keywords));
            sb.AppendFormatLine("DisplayName: {0}", victim.DisplayName);
            sb.AppendFormatLine("Description: {0}", victim.Description);
            if (victim.Leader != null)
                sb.AppendFormatLine("Leader: {0}", victim.Leader.DisplayName);
            if (nonPlayableVictim?.Master != null)
                sb.AppendFormatLine("Master: {0}", nonPlayableVictim.Master.DisplayName);
            if (playableVictim?.Group != null)
                foreach (IPlayableCharacter member in playableVictim.Group.Members)
                    sb.AppendFormatLine("Group member: {0}", member.DisplayName);
            if (playableVictim?.Pets.Any() == true)
                foreach(INonPlayableCharacter pet in playableVictim.Pets)
                    sb.AppendFormatLine("Pet: {0}", pet.DisplayName);
            if (victim.IncarnatedBy != null)
                sb.AppendFormatLine("Incarnated by {0}", victim.IncarnatedBy.DisplayName);
            else
                sb.AppendFormatLine("Incarnatable: {0}", victim.Incarnatable);
            if (playableVictim?.ImpersonatedBy != null)
                sb.AppendFormatLine("Impersonated by {0}", playableVictim.ImpersonatedBy.DisplayName);
            if (victim.Fighting != null)
                sb.AppendFormatLine("Fighting: {0}", victim.Fighting.DisplayName);
            sb.AppendFormatLine("Position: {0}", victim.Position);
            sb.AppendFormatLine("Furniture: {0}", victim.Furniture?.DisplayName ?? "(none)");
            sb.AppendFormatLine("Room: {0} [vnum: {1}]", victim.Room.DisplayName, victim.Room.Blueprint?.Id ?? -1);
            sb.AppendFormatLine("Race: {0} Class: {1}", victim.Race?.DisplayName ?? "(none)", victim.Class?.DisplayName ?? "(none)");
            sb.AppendFormatLine("Sex: {0} (base: {1})", victim.Sex, victim.BaseSex);
            sb.AppendFormatLine("Size: {0} (base: {1})", victim.Size, victim.BaseSize);
            sb.AppendFormatLine("Silver: {0} Gold: {1}", victim.SilverCoins, victim.GoldCoins);
            sb.AppendFormatLine("Carry: {0}/{1} Weight: {2}/{3}", victim.CarryNumber, victim.MaxCarryNumber, victim.CarryWeight, victim.MaxCarryWeight);
            if (playableVictim != null)
                sb.AppendFormatLine("Level: {0} Experience: {1} NextLevel: {2}", playableVictim.Level, playableVictim.Experience, playableVictim.ExperienceToLevel);
            else
                sb.AppendFormatLine("Level: {0}", victim.Level);
            if (nonPlayableVictim != null)
                sb.AppendFormatLine("Damage: {0}d{1}+{2} {3} {4}", nonPlayableVictim.DamageDiceCount, nonPlayableVictim.DamageDiceValue, nonPlayableVictim.DamageDiceBonus, nonPlayableVictim.DamageType, nonPlayableVictim.DamageNoun);
            sb.AppendFormatLine("Hitpoints: Current: {0} Max: {1}", victim.HitPoints, victim.MaxHitPoints);
            sb.AppendFormatLine("Movepoints: Current: {0} Max: {1}", victim.MovePoints, victim.MaxMovePoints);
            sb.AppendFormatLine("Flags: {0} (base: {1})", victim.CharacterFlags, victim.BaseCharacterFlags);
            sb.AppendFormatLine("Immunites: {0} (base: {1})  Resistances: {2} (base: {3})  Vulnerabilities: {4} (base: {5})", victim.Immunities, victim.BaseImmunities, victim.Resistances, victim.BaseResistances, victim.Vulnerabilities, victim.BaseVulnerabilities);
            sb.AppendFormatLine("Alignment: {0}", victim.Alignment);
            sb.AppendLine("Attributes:");
            foreach (CharacterAttributes attribute in EnumHelpers.GetValues<CharacterAttributes>())
                sb.AppendFormatLine("{0}: {1} (base: {2})", attribute, victim[attribute], victim.BaseAttribute(attribute));
            foreach (ResourceKinds resourceKind in EnumHelpers.GetValues<ResourceKinds>())
                sb.AppendFormatLine("{0}: {1} Max: {2}", resourceKind, victim[resourceKind], victim.MaxResource(resourceKind));
            if (nonPlayableVictim != null)
            {
                sb.AppendFormatLine("Act: {0}", nonPlayableVictim.ActFlags);
                sb.AppendFormatLine("Offensive: {0}", nonPlayableVictim.OffensiveFlags);
                sb.AppendFormatLine("Assist: {0}", nonPlayableVictim.AssistFlags);
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

        [AdminCommand("istat", "Information")]
        [AdminCommand("ostat", "Information")]
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
            sb.AppendFormatLine("Type: {0}", item.GetType());
            sb.AppendFormatLine("Name: {0} Keywords: {1}", item.Name, string.Join(",", item.Keywords));
            sb.AppendFormatLine("DisplayName: {0}", item.DisplayName);
            sb.AppendFormatLine("Description: {0}", item.Description);
            if (item.ExtraDescriptions != null)
            {
                foreach (var lookup in item.ExtraDescriptions)
                    foreach (string extraDescr in lookup)
                        sb.AppendFormatLine("ExtraDescription: {0} " + Environment.NewLine + "{1}", lookup.Key, extraDescr);
            }
            sb.AppendFormatLine("Type: {0}", item.GetType().Name);
            if (item.IncarnatedBy != null)
                sb.AppendFormatLine("Incarnated by {0}", item.IncarnatedBy.DisplayName);
            else
                sb.AppendFormatLine("Incarnatable: {0}", item.Incarnatable);
            if (item.ContainedInto != null)
                sb.AppendFormatLine("Contained in {0}", item.ContainedInto.DebugName);
            sb.AppendFormatLine("Equipped by {0} on {1}", item.EquippedBy?.DebugName ?? "(none)", item.WearLocation);
            if (item.NoTake)
                sb.Append("Cannot be taken");
            sb.AppendFormatLine("Level: {0}", item.Level);
            sb.AppendFormatLine("Cost: {0} Weight: {1}", item.Cost, item.Weight);
            sb.AppendFormatLine("CarryCount: {0} TotalWeight: {1}", item.CarryCount, item.TotalWeight);
            if (item.DecayPulseLeft > 0)
                sb.AppendFormatLine("Decay in {0}", StringHelpers.FormatDelay(item.DecayPulseLeft / Pulse.PulsePerSeconds));
            sb.AppendFormatLine("Flags: {0} (base: {1})", item.ItemFlags, item.BaseItemFlags);
            //
            if (item is IItemArmor armor)
                sb.AppendFormatLine("Bash: {0} Pierce: {1} Slash: {2} Exotic: {3}", armor.Bash, armor.Pierce, armor.Slash, armor.Exotic);
            //
            if (item is IItemCastSpellsCharge castSpellsCharge)
                sb.AppendFormatLine("Level: {0} Current: {1} Max: {2} Spell: {3} Already recharge: {4}", castSpellsCharge.SpellLevel, castSpellsCharge.CurrentChargeCount, castSpellsCharge.MaxChargeCount, castSpellsCharge.Spell?.Name ?? "-", castSpellsCharge.AlreadyRecharged);
            //
            if (item is IItemCastSpellsNoCharge castSpellsNoCharge)
                sb.AppendFormatLine("Level: {0} Spell1: {1} Spell2: {2} Spell3: {3} Spell4: {4}", castSpellsNoCharge.SpellLevel, castSpellsNoCharge.FirstSpell?.Name ?? "-", castSpellsNoCharge.SecondSpell?.Name ?? "-", castSpellsNoCharge.ThirdSpell?.Name ?? "-", castSpellsNoCharge.FourthSpell?.Name ?? "-");
            //
            if (item is IItemContainer container)
                sb.AppendFormatLine("Item count: {0} Weight: {1} MaxWeight: {2} Flags: {3} Key: {4} MaxWeightPerItem: {5} Weight multiplier: {6}", 
                    container.Content.Count(), container.Weight, container.MaxWeight, container.ContainerFlags, container.KeyId, container.MaxWeightPerItem, container.WeightMultiplier);
            //
            if (item is IItemCorpse)
                sb.AppendLine("No additional informations");
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
            if (item is IItemLight light)
                sb.AppendFormatLine("Time left: {0}", light.IsInfinite ? "Infinite" : StringHelpers.FormatDelay(light.TimeLeft * 60));
            //
            if (item is IItemMoney money)
                sb.AppendFormatLine("Silver: {0} Gold: {1}", money.SilverCoins, money.GoldCoins);
            //
            if (item is IItemPortal portal)
                sb.AppendFormatLine("Destination: {0} Flags: {1} Key: {2} Current charges: {3} Max charges: {4}", portal.Destination?.DebugName ?? "???", portal.PortalFlags, portal.KeyId, portal.CurrentChargeCount, portal.MaxChargeCount);
            //
            if (item is IItemShield shield)
                sb.AppendFormatLine("Armor: {0}", shield.Armor);
            //
            if (item is IItemWeapon weapon)
                sb.AppendFormatLine("Weapon type: {0}  {1}d{2} {3} {4} (base: {5}) {6}", weapon.Type, weapon.DiceCount, weapon.DiceValue, weapon.DamageType, weapon.WeaponFlags, weapon.BaseWeaponFlags, weapon.DamageNoun);

            // TODO: other item type
            //
            AppendAuras(sb, item.Auras);
            //
            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("cfind", "Information")]
        [AdminCommand("mfind", "Information")]
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

        [AdminCommand("ifind", "Information")]
        [AdminCommand("ofind", "Information")]
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

        [AdminCommand("cinfo", "Information")]
        [AdminCommand("minfo", "Information")]
        [Syntax("[cmd] <id>")]
        protected virtual CommandExecutionResults DoCinfo(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            if (!parameters[0].IsNumber)
                return CommandExecutionResults.SyntaxError;

            int id = parameters[0].AsNumber;
            CharacterBlueprintBase blueprint = World.GetCharacterBlueprint(id);
            if (blueprint == null)
            {
                Send("Not found.");
                return CommandExecutionResults.TargetNotFound;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormatLine("Id: {0} Type: {1}", blueprint.Id, blueprint.GetType());
            sb.AppendFormatLine("Name: {0}", blueprint.Name);
            sb.AppendFormatLine("ShortDescription: {0}", blueprint.ShortDescription);
            sb.AppendFormatLine("LongDescription: {0}", blueprint.LongDescription);
            sb.AppendFormatLine("Description: {0}", blueprint.Description);
            sb.AppendFormatLine("Level: {0} Sex: {1}", blueprint.Level, blueprint.Sex);
            sb.AppendFormatLine("Wealth: {0} Alignment {1}", blueprint.Wealth, blueprint.Alignment);
            sb.AppendFormatLine("Damage: {0}d{1}+{2} DamageType: {3} DamageNoun: {4}", blueprint.DamageDiceCount, blueprint.DamageDiceValue, blueprint.DamageDiceBonus, blueprint.DamageType, blueprint.DamageNoun);
            sb.AppendFormatLine("Hitpoints: {0}d{1}+{2}", blueprint.HitPointDiceCount, blueprint.HitPointDiceValue, blueprint.HitPointDiceBonus);
            sb.AppendFormatLine("Mana: {0}d{1}+{2}", blueprint.ManaDiceCount, blueprint.ManaDiceValue, blueprint.ManaDiceBonus);
            sb.AppendFormatLine("Hit roll: {0}", blueprint.HitRollBonus);
            sb.AppendFormatLine("Bash: {0} Pierce: {1} Slash: {2} Exotic: {3}", blueprint.ArmorBash, blueprint.ArmorPierce, blueprint.ArmorSlash, blueprint.ArmorExotic);
            sb.AppendFormatLine("Flags: {0}", blueprint.CharacterFlags);
            sb.AppendFormatLine("Offensive: {0}", blueprint.OffensiveFlags);
            sb.AppendFormatLine("Act: {0}", blueprint.ActFlags);
            sb.AppendFormatLine("Immunities: {0}", blueprint.Immunities);
            sb.AppendFormatLine("Resistances: {0}", blueprint.Resistances);
            sb.AppendFormatLine("Vulnerabilities: {0}", blueprint.Vulnerabilities);
            // TODO: loot table, script
            // TODO: specific blueprint

            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("iinfo", "Information")]
        [AdminCommand("oinfo", "Information")]
        [Syntax("[cmd] <id>")]
        protected virtual CommandExecutionResults DoIinfo(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            if (!parameters[0].IsNumber)
                return CommandExecutionResults.SyntaxError;

            int id = parameters[0].AsNumber;
            ItemBlueprintBase blueprint = World.GetItemBlueprint(id);
            if (blueprint == null)
            {
                Send("Not found.");
                return CommandExecutionResults.TargetNotFound;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormatLine("Id: {0} Type: {1}", blueprint.Id, blueprint.GetType());
            sb.AppendFormatLine("Name: {0}", blueprint.Name);
            sb.AppendFormatLine("ShortDescription: {0}", blueprint.ShortDescription);
            sb.AppendFormatLine("Description: {0}", blueprint.Description);
            sb.AppendFormatLine("Level: {0} Weight: {1}", blueprint.Level, blueprint.Weight);
            sb.AppendFormatLine("Cost: {0} NoTake: {1}", blueprint.Cost, blueprint.NoTake);
            sb.AppendFormatLine("Flags: {0} WearLocation: {1}", blueprint.ItemFlags, blueprint.WearLocation);
            if (blueprint.ExtraDescriptions != null)
            {
                foreach (var lookup in blueprint.ExtraDescriptions)
                    foreach (string extraDescr in lookup)
                        sb.AppendFormatLine("ExtraDescription: {0} " + Environment.NewLine + "{1}", lookup.Key, extraDescr);
            }
            switch (blueprint)
            {
                case ItemCastSpellsNoChargeBlueprintBase noChargeBlueprint: // pill, potion, scroll
                    sb.AppendFormatLine("Level: {0} Spell1: {1} Spell2: {2} Spell3: {3} Spell4: {4}", noChargeBlueprint.SpellLevel, noChargeBlueprint.Spell1, noChargeBlueprint.Spell2, noChargeBlueprint.Spell3, noChargeBlueprint.Spell4);
                    break;
                case ItemCastSpellsChargeBlueprintBase chargeBlueprint: // wand, staff
                    sb.AppendFormatLine("Level: {0} #MaxCharge: {1} #CurrentCharge: {2} Spell: {3} AlreadyRecharged: {4}", chargeBlueprint.SpellLevel, chargeBlueprint.MaxChargeCount, chargeBlueprint.CurrentChargeCount, chargeBlueprint.Spell, chargeBlueprint.AlreadyRecharged);
                    break;

                case ItemArmorBlueprint armor:
                    sb.AppendFormatLine("Bash: {0} Pierce: {1} Slash: {2} Exotic: {3}", armor.Bash, armor.Pierce, armor.Slash, armor.Exotic);
                    break;
                case ItemBoatBlueprint _:
                    break;
                case ItemContainerBlueprint container:
                    sb.AppendFormatLine("MaxWeight: {0} Key: {1} MaxWeightPerItem: {2} WeightMultiplier: {3} Flags: {4}", container.MaxWeight, container.Key, container.MaxWeightPerItem, container.WeightMultiplier, container.ContainerFlags);
                    break;
                case ItemCorpseBlueprint _:
                    break;
                case ItemDrinkContainerBlueprint drinkContainer:
                    sb.AppendFormatLine("MaxLiquid: {0} CurrentLight: {1} LiquidType: {2} IsPoisoned: {3}", drinkContainer.MaxLiquidAmount, drinkContainer.CurrentLiquidAmount, drinkContainer.LiquidType, drinkContainer.IsPoisoned);
                    break;
                case ItemFoodBlueprint foodBlueprint:
                    sb.AppendFormatLine("FullHours: {0} HungerHours: {1} IsPoisoned: {2}", foodBlueprint.FullHours, foodBlueprint.HungerHours, foodBlueprint.IsPoisoned);
                    break;
                case ItemFountainBlueprint fountainBlueprint:
                    sb.AppendFormatLine("LiquidType: {0}", fountainBlueprint.LiquidType);
                    break;
                case ItemFurnitureBlueprint furnitureBlueprint:
                    sb.AppendFormatLine("MaxPeople: {0} MaxWeight: {1} Action: {2} Place: {3} HealBonus: {4} ResourceBonus: {5}", furnitureBlueprint.MaxPeople, furnitureBlueprint.MaxWeight, furnitureBlueprint.FurnitureActions, furnitureBlueprint.FurniturePlacePreposition, furnitureBlueprint.HealBonus, furnitureBlueprint.ResourceBonus);
                    break;
                case ItemJewelryBlueprint _:
                    break;
                case ItemKeyBlueprint _:
                    break;
                case ItemLightBlueprint lightBlueprint:
                    sb.AppendFormatLine("DurationHours: {0}", lightBlueprint.DurationHours);
                    break;
                case ItemMoneyBlueprint moneyBlueprint:
                    sb.AppendFormatLine("Silver: {0} Gold: {0}", moneyBlueprint.SilverCoins, moneyBlueprint.GoldCoins);
                    break;
                case ItemPortalBlueprint portalBlueprint:
                    sb.AppendFormatLine("Destination: {0} Key: {1} Flags: {2} #MaxCharge: {3} #CurrentCharge: {4}", portalBlueprint.Destination, portalBlueprint.Key, portalBlueprint.PortalFlags, portalBlueprint.MaxChargeCount, portalBlueprint.CurrentChargeCount);
                    break;
                case ItemQuestBlueprint _:
                    break;
                case ItemShieldBlueprint shieldBlueprint:
                    sb.AppendFormatLine("Armor: {0}", shieldBlueprint.Armor);
                    break;
                case ItemTreasureBlueprint _:
                    break;
                case ItemWarpStoneBlueprint _:
                    break;
                case ItemWeaponBlueprint weaponBlueprint:
                    sb.AppendFormatLine("WeaponType: {0} damage: {1}d{2} DamageType: {3} Flags: {4} DamageNoun: {5}", weaponBlueprint.Type, weaponBlueprint.DiceCount, weaponBlueprint.DiceValue, weaponBlueprint.DamageType, weaponBlueprint.Flags, weaponBlueprint.DamageNoun);
                    break;
            }

            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("resets", "Information", Priority = 50)]
        [Syntax(
            "[cmd] <id>",
            "[cmd] (if impersonated)")]
        protected virtual CommandExecutionResults DoResets(string rawParameters, params CommandParameter[] parameters)
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

            if (room.Blueprint.Resets == null || !room.Blueprint.Resets.Any())
            {
                Send("No resets.");
                return CommandExecutionResults.Ok;
            }

            Send(" No.  Loads    Description       Location         Vnum    Max   Description");
            Send("==== ======== ============= =================== ======== [R  W] ===========");
            StringBuilder sb = new StringBuilder();
            int resetId = 0;
            CharacterBlueprintBase previousCharacter = null;
            ItemBlueprintBase previousItem = null;
            foreach (ResetBase reset in room.Blueprint.Resets)
            {
                sb.AppendFormat("[{0,2}] ", resetId);
                switch (reset)
                {
                    case CharacterReset characterReset: // 'M'
                    {
                        CharacterBlueprintBase characterBlueprint = World.GetCharacterBlueprint(characterReset.CharacterId);
                        if (characterBlueprint == null)
                        {
                            sb.AppendFormatLine("Load Char - Bad Character {0}", characterReset.CharacterId);
                            continue;
                        }

                        RoomBlueprint roomBlueprint = World.GetRoomBlueprint(characterReset.RoomId);
                        if (roomBlueprint == null)
                        {
                            sb.AppendFormatLine("Load Char - Bad Room {0}", characterReset.RoomId);
                            continue;
                        }

                        previousCharacter = characterBlueprint;
                        sb.AppendFormatLine("M[{0,5}] {1,-13} in room             R[{2,5}] [{3,-2}{4,2}] {5,-15}", characterReset.CharacterId, characterBlueprint.ShortDescription.MaxLength(13), characterReset.RoomId, characterReset.LocalLimit, characterReset.GlobalLimit, roomBlueprint.Name.MaxLength(15));
                        break;
                    }

                    case ItemInRoomReset itemInRoomReset: // 'O'
                    {
                        ItemBlueprintBase itemBlueprint = World.GetItemBlueprint(itemInRoomReset.ItemId);
                        if (itemBlueprint == null)
                        {
                            sb.AppendFormatLine("Load Item - Bad Item {0}", itemInRoomReset.ItemId);
                            continue;
                        }

                        previousItem = itemBlueprint;

                        RoomBlueprint roomBlueprint = World.GetRoomBlueprint(itemInRoomReset.RoomId);
                        if (roomBlueprint == null)
                        {
                            sb.AppendFormatLine("Load Item - Bad Room {0}", itemInRoomReset.RoomId);
                            continue;
                        }

                        sb.AppendFormatLine("O[{0,5}] {1,-13} in room             R[{2,5}] [{3,-2}{4,2}] {5,-15}", itemInRoomReset.ItemId, itemBlueprint.ShortDescription.MaxLength(13), itemInRoomReset.RoomId, itemInRoomReset.LocalLimit, itemInRoomReset.GlobalLimit, roomBlueprint.Name.MaxLength(15));
                            break;
                    }

                    case ItemInItemReset itemInItemReset: // 'P'
                    {
                        ItemBlueprintBase itemBlueprint = World.GetItemBlueprint(itemInItemReset.ItemId);
                        if (itemBlueprint == null)
                        {
                            sb.AppendFormatLine("Put Item - Bad Item {0}", itemInItemReset.ItemId);
                            continue;
                        }

                        previousItem = itemBlueprint;

                        ItemBlueprintBase containerBlueprint = World.GetItemBlueprint(itemInItemReset.ContainerId);
                        if (containerBlueprint == null)
                        {
                            sb.AppendFormatLine("Put Item - Bad To Item {0}", itemInItemReset.ContainerId);
                            continue;
                        }

                        sb.AppendFormatLine("O[{0,5}] {1,-13} inside              O[{2,5}]       {3,-15}", itemInItemReset.ItemId, itemBlueprint.ShortDescription.MaxLength(13), itemInItemReset.ContainerId, containerBlueprint.ShortDescription.MaxLength(15));
                        break;
                    }

                    case ItemInCharacterReset itemInCharacterReset: // 'G'
                    {
                        ItemBlueprintBase itemBlueprint = World.GetItemBlueprint(itemInCharacterReset.ItemId);
                        if (itemBlueprint == null)
                        {
                            sb.AppendFormatLine("Give Item - Bad Item {0}", itemInCharacterReset.ItemId);
                            continue;
                        }

                        previousItem = itemBlueprint;

                        if (previousCharacter == null)
                        {
                            sb.AppendFormatLine("Give Item - No Previous Character");
                            continue;
                        }

                        sb.AppendFormatLine("O[{0,5}] {1,-13} {2,-19} M[{3,5}]       {4,-15}", itemInCharacterReset.ItemId, itemBlueprint.ShortDescription.MaxLength(13), "in the inventory", previousCharacter.Id, previousCharacter.ShortDescription.MaxLength(15));
                        break;
                    }

                    case ItemInEquipmentReset itemInEquipmentReset: // 'E'
                    {
                        ItemBlueprintBase itemBlueprint = World.GetItemBlueprint(itemInEquipmentReset.ItemId);
                        if (itemBlueprint == null)
                        {
                            sb.AppendFormatLine("Equip Item - Bad Item {0}", itemInEquipmentReset.ItemId);
                            continue;
                        }

                        previousItem = itemBlueprint;

                        if (previousCharacter == null)
                        {
                            sb.AppendFormatLine("Equip Item - No Previous Character");
                            continue;
                        }

                        sb.AppendFormatLine("O[{0,5}] {1,-13} {2,-19} M[{3,5}]       {4,-15}", itemInEquipmentReset.ItemId, itemBlueprint.ShortDescription.MaxLength(13), itemInEquipmentReset.EquipmentSlot, previousCharacter.Id, previousCharacter.ShortDescription.MaxLength(15));
                        break;
                        }

                    default:
                        sb.AppendFormatLine("Bad reset command: {0}.", reset.GetType());
                        break;
                }
                resetId++;
            }
            Send(sb);
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

        [AdminCommand("info", "Information")]
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
                        $"Resource(s): {string.Join(",", matchingClass.ResourceKinds?.Select(x => $"{x.ResourceColor()}") ?? Enumerable.Empty<string>())}",
                        $"Prime attribute: %W%{matchingClass.PrimeAttribute}%x%",
                        $"Max practice percentage: %W%{matchingClass.MaxPracticePercentage}%x%",
                        $"Hp/level: min: %W%{matchingClass.MinHitPointGainPerLevel}%x% max: %W%{matchingClass.MaxHitPointGainPerLevel}%x%"
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
                        $"ShortName: {matchingRace.ShortName}",
                        $"Immunities: %y%{matchingRace.Immunities}%x%",
                        $"Resistances: %b%{matchingRace.Resistances}%x%",
                        $"Vulnerabilities: %r%{matchingRace.Vulnerabilities}%x%",
                        $"Size: %M%{matchingRace.Size}%x%",
                        $"Exp/Level:     %W%{string.Join(" ", ClassManager.Classes.Select(x => $"{x.ShortName,5}"))}%x%",
                        $"               %r%{string.Join(" ", ClassManager.Classes.Select(x => $"{matchingRace.ClassExperiencePercentageMultiplier(x)*10,5}"))}%x%", // *10 because base experience is 1000
                        $"Attributes:       %Y%{string.Join(" ", EnumHelpers.GetValues<BasicAttributes>().Select(x => $"{x.ShortName(),3}"))}%x%",
                        $"Attributes start: %c%{string.Join(" ", EnumHelpers.GetValues<BasicAttributes>().Select(x => $"{matchingRace.GetStartAttribute((CharacterAttributes)x),3}"))}%x%",
                        $"Attributes max:   %B%{string.Join(" ", EnumHelpers.GetValues<BasicAttributes>().Select(x => $"{matchingRace.GetMaxAttribute((CharacterAttributes)x),3}"))}%x%",
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
                else if (item.EquippedBy != null)
                {
                    sb.Append(" equipped by ");
                    sb.Append("<");
                    sb.Append(DisplayEntityAndContainer(item.EquippedBy));
                    sb.Append(">");
                }
                else
                    sb.Append("Seems to be nowhere!!!");
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
