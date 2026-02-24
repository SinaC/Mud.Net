using Mud.Blueprints.Character;
using Mud.Blueprints.Quest;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Extensions;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("cstat", "Information")]
[Alias("mstat")]
[Syntax("[cmd] <character>")]
public class Cstat : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new RequiresAtLeastOneArgument()];

    private ICharacterManager CharacterManager { get; }

    public Cstat(ICharacterManager characterManager)
    {
        CharacterManager = characterManager;
    }

    private ICharacter Whom { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Whom = Impersonating == null
            ? FindHelpers.FindByName(CharacterManager.Characters, actionInput.Parameters[0])!
            : FindHelpers.FindChararacterInWorld(CharacterManager, Impersonating, actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.NotFound;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var npcWhom = Whom as INonPlayableCharacter;
        var pcWhom = Whom as IPlayableCharacter;
        StringBuilder sb = new();
        if (npcWhom?.Blueprint != null)
        {
            sb.AppendFormatLine("Blueprint: {0}", npcWhom.Blueprint.Id);
            // TODO: display blueprint
            if (npcWhom.Blueprint is CharacterQuestorBlueprint characterQuestorBlueprint)
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
            if (npcWhom.Blueprint is CharacterShopBlueprint characterShopBlueprint)
            {
                sb.AppendLine("Shopkeeper:");
                sb.AppendFormatLine("BuyTypes: {0}", string.Join(",", characterShopBlueprint.BuyBlueprintTypes.Select(x => x.ToString().AfterLast('.').Replace("Blueprint", string.Empty))));
                sb.AppendFormatLine("Profit buy: {0}% sell: {1}%", characterShopBlueprint.ProfitBuy, characterShopBlueprint.ProfitSell);
                sb.AppendFormatLine("Open hour: {0} Close hour: {1}", characterShopBlueprint.OpenHour, characterShopBlueprint.CloseHour);
            }
            if (npcWhom.Blueprint is CharacterPetShopBlueprint characterPetShopBlueprint)
            {
                sb.AppendLine("Pet shopkeeper:");
                sb.AppendFormatLine("Pets: {0}", string.Join(",", characterPetShopBlueprint.PetBlueprints.Select(x => $"{x.Name} (id: {x.Id})")));
                sb.AppendFormatLine("Profit buy: {0}% sell: {1}%", characterPetShopBlueprint.ProfitBuy, characterPetShopBlueprint.ProfitSell);
                sb.AppendFormatLine("Open hour: {0} Close hour: {1}", characterPetShopBlueprint.OpenHour, characterPetShopBlueprint.CloseHour);
            }
        }
        else
            sb.AppendLine("No blueprint");
        sb.AppendFormatLine("Name: {0} Keywords: {1}", Whom.Name, string.Join(",", Whom.Keywords));
        sb.AppendFormatLine("DisplayName: {0}", Whom.DisplayName);
        sb.AppendFormatLine("Leader: {0} Master: {1}", Whom.Leader?.DisplayName ?? "/", npcWhom?.Master?.DisplayName ?? "/");
        if (pcWhom?.Group != null)
        {
            foreach (IPlayableCharacter member in pcWhom.Group.Members)
                sb.AppendFormatLine("Group member: {0}", member.DisplayName);
        }
        if (pcWhom?.Pets.Any() == true)
        {
            foreach (INonPlayableCharacter pet in pcWhom.Pets)
                sb.AppendFormatLine("Pet: {0}", pet.DisplayName);
        }
        if (Whom.IncarnatedBy != null)
            sb.AppendFormatLine("Incarnated by {0}", Whom.IncarnatedBy.DisplayName);
        else
            sb.AppendFormatLine("Incarnatable: {0}", Whom.Incarnatable);
        if (pcWhom?.ImpersonatedBy != null)
        {
            sb.AppendFormatLine("Impersonated by {0} Immortal mode: {1}", pcWhom.ImpersonatedBy.DisplayName, pcWhom.ImmortalMode);
        }
        if (Whom.Fighting != null)
            sb.AppendFormatLine("Fighting: {0}", Whom.Fighting.DisplayName);
        sb.AppendFormatLine("Shape: {0}", Whom.Shape);
        sb.AppendFormatLine("Position: {0} GCD:{1} Daze:{2} Stunned: {3}", Whom.Position, Whom.GlobalCooldown, Whom.Daze, Whom.Stun);
        sb.AppendFormatLine("Furniture: {0}", Whom.Furniture?.DisplayName ?? "(none)");
        sb.AppendFormatLine("Room: {0} [vnum: {1}]", Whom.Room?.DisplayName ?? "(none)", Whom.Room?.Blueprint.Id ?? -1);
        sb.AppendFormatLine("Race: {0} (base: {1}) Class: {2}", Whom.Race?.DisplayName ?? "(none)", Whom.BaseRace?.DisplayName ?? "(none)", Whom.Classes.DisplayName());
        sb.AppendFormatLine("Sex: {0} (base: {1})", Whom.Sex, Whom.BaseSex);
        sb.AppendFormatLine("Size: {0} (base: {1})", Whom.Size, Whom.BaseSize);
        sb.AppendFormatLine("Silver: {0} Gold: {1}", Whom.SilverCoins, Whom.GoldCoins);
        sb.AppendFormatLine("Carry: {0}/{1} Weight: {2}/{3}", Whom.CarryNumber, Whom.MaxCarryNumber, Whom.CarryWeight, Whom.MaxCarryWeight);
        if (pcWhom != null)
            sb.AppendFormatLine("Level: {0} Experience: {1} NextLevel: {2}", pcWhom.Level, pcWhom.Experience, pcWhom.ExperienceToLevel);
        else
            sb.AppendFormatLine("Level: {0}", Whom.Level);
        if (npcWhom != null)
            sb.AppendFormatLine("Damage: {0}d{1}+{2} {3} {4}", npcWhom.DamageDiceCount, npcWhom.DamageDiceValue, npcWhom.DamageDiceBonus, npcWhom.DamageType, npcWhom.DamageNoun);
        sb.AppendFormatLine("Hitpoints: Current: {0} Max: {1}", Whom[ResourceKinds.HitPoints], Whom.MaxResource(ResourceKinds.HitPoints));
        sb.AppendFormatLine("Movepoints: Current: {0} Max: {1}", Whom[ResourceKinds.MovePoints], Whom.MaxResource(ResourceKinds.MovePoints));
        sb.AppendFormatLine("Flags: {0} (base: {1})", Whom.CharacterFlags, Whom.BaseCharacterFlags);
        sb.AppendFormatLine("Immunites: {0} (base: {1})", Whom.Immunities, Whom.BaseImmunities);
        sb.AppendFormatLine("Resistances: {0} (base: {1})", Whom.Resistances, Whom.BaseResistances);
        sb.AppendFormatLine("Vulnerabilities: {0} (base: {1})", Whom.Vulnerabilities, Whom.BaseVulnerabilities);
        sb.AppendFormatLine("Shields: {0} (base: {1})", Whom.ShieldFlags, Whom.BaseShieldFlags);
        sb.AppendFormatLine("Forms: {0} (base: {1})", Whom.BodyForms, Whom.BaseBodyForms);
        sb.AppendFormatLine("Parts: {0} (base: {1})", Whom.BodyParts, Whom.BaseBodyParts);
        sb.AppendFormatLine("Alignment: {0}", Whom.Alignment);
        sb.AppendFormatLine("Attributes: {0}", string.Join(",", Enum.GetValues<CharacterAttributes>().Select(x => $"{x.ShortName()}: {Whom[x]} ({Whom.BaseAttribute(x)})")));
        sb.AppendFormatLine("Resources: {0}", string.Join(",", Enum.GetValues<ResourceKinds>().Where(x => !x.IsMandatoryResource()).Select(x => $"{x.DisplayName()}: {Whom[x]} Max: {Whom.MaxResource(x)} [{(Whom.CurrentResourceKinds.Contains(x) ? "Y" : "N")}]")));
        if (npcWhom != null)
        {
            sb.AppendFormatLine("Act: {0}", npcWhom.ActFlags);
            sb.AppendFormatLine("Offensive: {0}", npcWhom.OffensiveFlags);
            sb.AppendFormatLine("Assist: {0}", npcWhom.AssistFlags);
            if (npcWhom.SpecialBehavior != null)
                sb.AppendFormatLine("Special: {0}", npcWhom.SpecialBehavior.GetType().Name.AfterLast('.') ?? "???"); // TODO: name ?
        }
        if (npcWhom?.Blueprint != null)
        {
            sb.AppendFormatLine("ShortDescription: {0}", npcWhom.Blueprint.ShortDescription);
            sb.AppendFormat("LongDescription: {0}", npcWhom.Blueprint.LongDescription);
        }
        sb.AppendFormat("Description: {0}", Whom.Description);
        if (npcWhom?.Blueprint != null)
        {
            if (npcWhom.Blueprint.MobProgramTriggers.Count > 0)
            {
                sb.AppendFormat("MobPrograms:");
                foreach (var trigger in npcWhom.Blueprint.MobProgramTriggers)
                    sb.AppendFormatLine("Trigger: {0} MobProgram: {1}", trigger.ToString() ?? string.Empty, trigger.MobProgramId);
            }
        }
        if (pcWhom != null)
        {
            sb.Append("Conditions: ");
            sb.AppendLine(string.Join(" ", Enum.GetValues<Conditions>().Select(x => $"{x}: {pcWhom[x]}")));
        }
        foreach (IAura aura in Whom.Auras)
            aura.Append(sb);
        if (Whom.Equipments.Any(x => x.Item != null))
        {
            sb.AppendLine("Equipments:");
            foreach (IEquippedItem equippedItem in Whom.Equipments.Where(x => x.Item != null))
            {
                sb.Append(equippedItem.EquipmentSlotsToString(Whom.Size));
                sb.AppendLine($"{equippedItem.Item!.DisplayName} [id: {equippedItem.Item!.Blueprint.Id.ToString() ?? " ??? "}]");
            }
        }
        if (Whom.Inventory.Any())
        {
            sb.AppendLine("Inventory:");
            foreach (var item in Whom.Inventory)
                sb.AppendLine($"{item.DisplayName} [id: {item.Blueprint.Id.ToString() ?? " ??? "}]");
        }
        if (pcWhom != null)
        {
            AppendQuests(sb, pcWhom);
        }
        //
        Actor.Send(sb);
    }

    private static void AppendQuests(StringBuilder sb, IPlayableCharacter pc)
    {
        if (pc.ActiveQuests.Any())
        {
            sb.AppendLine("Quests:");
            int id = 0;
            foreach (var quest in pc.ActiveQuests)
            {
                BuildQuestSummary(sb, pc, quest, id);
                id++;
            }
        }
        else
            sb.AppendLine("No quest");
    }

    private static void BuildQuestSummary(StringBuilder sb, IPlayableCharacter pc, IQuest quest, int? id)
    {
        var questType = quest is IGeneratedQuest ? "[AUTO] " : string.Empty;
        var difficultyColor = StringHelpers.DifficultyColor(pc.Level, quest.Level);
        // TODO: Table ?
        sb.Append($"{id + 1,2}) {questType}{difficultyColor}{quest.Title}%x%: {(quest.AreObjectivesFulfilled ? "%g%complete%x%" : "in progress")}");
        if (quest.TimeLimit > 0)
            sb.Append($" Time left: {Pulse.ToTimeSpan(quest.PulseLeft).FormatDelay()}");
        if (quest is IGeneratedQuest generatedQuest)
            sb.Append($" Room: {generatedQuest.Room.DisplayName} [{generatedQuest.Room.Blueprint.Id}]");
        sb.AppendLine();
        if (!quest.AreObjectivesFulfilled)
            BuildQuestObjectives(sb, quest);
    }

    private static void BuildQuestObjectives(StringBuilder sb, IQuest quest)
    {
        foreach (var objective in quest.Objectives)
        {
            // TODO: 2 columns ?
            if (objective.IsCompleted)
                sb.AppendLine($"     %g%{objective.CompletionState}%x%");
            else
                sb.AppendLine($"     {objective.CompletionState}");
        }
    }
}
