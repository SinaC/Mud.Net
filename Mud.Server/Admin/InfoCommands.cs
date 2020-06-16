using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.DataStructures.HeapPriorityQueue;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Reset;
using Mud.Server.Blueprints.Room;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;

// ReSharper disable UnusedMember.Global

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [AdminCommand("abilities", "Information")]
        [Syntax(
            "[cmd]",
            "[cmd] <class>",
            "[cmd] <race>")]
        protected virtual CommandExecutionResults DoAbilities(string rawParameters, params ICommandParameter[] parameters)
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
        protected virtual CommandExecutionResults DoSpells(string rawParameters, params ICommandParameter[] parameters)
        {
            bool displayed = DisplayAbilitiesList(AbilityTypes.Spell, parameters);

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
            bool displayed = DisplayAbilitiesList(AbilityTypes.Spell, parameters);

            if (!displayed)
                return CommandExecutionResults.SyntaxError;
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("cfind", "Information")]
        [AdminCommand("mfind", "Information")]
        [Syntax("[cmd] <character>")]
        protected virtual CommandExecutionResults DoCfind(string rawParameters, params ICommandParameter[] parameters)
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
        protected virtual CommandExecutionResults DoIfind(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Searching items '{parameters[0].Value}'");
            List<IItem> items = FindHelpers.FindAllByName(ItemManager.Items, parameters[0]).OrderBy(x => x.Blueprint?.Id).ToList();
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
        protected virtual CommandExecutionResults DoCinfo(string rawParameters, params ICommandParameter[] parameters)
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
            sb.AppendFormatLine("Race: {0} Class: {1}", blueprint.Race, blueprint.Class);
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
            switch (blueprint)
            {
                case CharacterQuestorBlueprint characterQuestorBlueprint:
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
                    break;
                case CharacterShopBlueprint characterShopBlueprint:
                    sb.AppendLine("Shopkeeper:");
                    sb.AppendFormatLine("BuyTypes: {0}", string.Join(",", characterShopBlueprint.BuyBlueprintTypes.Select(x => x.ToString().AfterLast('.').Replace("Blueprint", string.Empty))));
                    sb.AppendFormatLine("Profit buy: {0}% sell: {1}%", characterShopBlueprint.ProfitBuy, characterShopBlueprint.ProfitSell);
                    sb.AppendFormatLine("Open hour: {0} Close hour: {1}", characterShopBlueprint.OpenHour, characterShopBlueprint.CloseHour);
                    break;
            }

            Send(sb);
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("iinfo", "Information")]
        [AdminCommand("oinfo", "Information")]
        [Syntax("[cmd] <id>")]
        protected virtual CommandExecutionResults DoIinfo(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            if (!parameters[0].IsNumber)
                return CommandExecutionResults.SyntaxError;

            int id = parameters[0].AsNumber;
            ItemBlueprintBase blueprint = ItemManager.GetItemBlueprint(id);
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
        protected virtual CommandExecutionResults DoResets(string rawParameters, params ICommandParameter[] parameters)
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
                room = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == id);
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

                        RoomBlueprint roomBlueprint = RoomManager.GetRoomBlueprint(characterReset.RoomId);
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
                        ItemBlueprintBase itemBlueprint = ItemManager.GetItemBlueprint(itemInRoomReset.ItemId);
                        if (itemBlueprint == null)
                        {
                            sb.AppendFormatLine("Load Item - Bad Item {0}", itemInRoomReset.ItemId);
                            continue;
                        }

                        previousItem = itemBlueprint;

                        RoomBlueprint roomBlueprint = RoomManager.GetRoomBlueprint(itemInRoomReset.RoomId);
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
                        ItemBlueprintBase itemBlueprint = ItemManager.GetItemBlueprint(itemInItemReset.ItemId);
                        if (itemBlueprint == null)
                        {
                            sb.AppendFormatLine("Put Item - Bad Item {0}", itemInItemReset.ItemId);
                            continue;
                        }

                        previousItem = itemBlueprint;

                        ItemBlueprintBase containerBlueprint = ItemManager.GetItemBlueprint(itemInItemReset.ContainerId);
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
                        ItemBlueprintBase itemBlueprint = ItemManager.GetItemBlueprint(itemInCharacterReset.ItemId);
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
                        ItemBlueprintBase itemBlueprint = ItemManager.GetItemBlueprint(itemInEquipmentReset.ItemId);
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
        protected virtual CommandExecutionResults DoPath(string rawParameters, params ICommandParameter[] parameters)
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
        protected virtual CommandExecutionResults DoInfo(string rawParameters, params ICommandParameter[] parameters)
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
                StringBuilder sb = TableGenerators.PlayableRaceTableGenerator.Value.Generate("Races", RaceManager.PlayableRaces);
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
                    matchingClass.Abilities.OrderBy(x => x.Level).ThenBy(x => x.Name));
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            // race name
            IPlayableRace matchingRace = RaceManager.PlayableRaces.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value));
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
                    matchingRace.Abilities.OrderBy(x => x.Level).ThenBy(x => x.Name));
                Page(sb);
                return CommandExecutionResults.Ok;
            }

            return CommandExecutionResults.SyntaxError;
        }

        //*********************** Helpers ***************************

        private bool DisplayAbilitiesList(AbilityTypes? filterOnAbilityType, params ICommandParameter[] parameters)
        {
            string title;
            if (filterOnAbilityType.HasValue)
            {
                switch (filterOnAbilityType.Value)
                {
                    case AbilityTypes.Passive: title = "Passives"; break;
                    case AbilityTypes.Spell: title = "Spells"; break;
                    case AbilityTypes.Skill: title = "Skills"; break;
                    default: title = "???"; break;
                }
            }
            else
                title = "Abilities";
            if (parameters.Length == 0)
            {
                // no filter
                StringBuilder sb = TableGenerators.FullInfoAbilityTableGenerator.Value.Generate(title, AbilityManager.Abilities
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
                    .ThenBy(x => x.Name));
                Page(sb);
                return true;
            }

            // filter on race?
            IPlayableRace matchingRace = RaceManager.PlayableRaces.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (matchingRace != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate($"{title} for {matchingRace.DisplayName}", matchingRace.Abilities
                    .OrderBy(x => x.Level)
                    .ThenBy(x => x.Name));
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
                    sb.Insert(0, previous.Item2.ShortExitDirections());
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
