using Mud.Blueprints.Character;
using Mud.Blueprints.Quest;
using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("cinfo", "Information")]
[Alias("minfo")]
[Syntax("[cmd] <id>")]
public class Cinfo : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new RequiresAtLeastOneArgument()];

    private ICharacterManager CharacterManager { get; }

    public Cinfo(ICharacterManager characterManager)
    {
        CharacterManager = characterManager;
    }

    private CharacterBlueprintBase Blueprint { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (!actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();

        int id = actionInput.Parameters[0].AsNumber;
        Blueprint = CharacterManager.GetCharacterBlueprint(id)!;
        if (Blueprint == null)
            return "Not found.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new();
        sb.AppendFormatLine("Id: {0} Type: {1}", Blueprint.Id, Blueprint.GetType().Name.AfterLast('.'));
        sb.AppendFormatLine("Name: {0}", Blueprint.Name);
        sb.AppendFormatLine("ShortDescription: {0}", Blueprint.ShortDescription);
        sb.AppendFormat("LongDescription: {0}", Blueprint.LongDescription);
        sb.AppendFormat("Description: {0}", Blueprint.Description);
        sb.AppendFormatLine("Level: {0} Group: {1} Sex: {2}", Blueprint.Level, Blueprint.Group, Blueprint.Sex);
        sb.AppendFormatLine("Race: {0} Class: {1}", Blueprint.Race, Blueprint.Class);
        sb.AppendFormatLine("Wealth: {0} Alignment {1}", Blueprint.Wealth, Blueprint.Alignment);
        sb.AppendFormatLine("Damage: {0}d{1}+{2} DamageType: {3} DamageNoun: {4}", Blueprint.DamageDiceCount, Blueprint.DamageDiceValue, Blueprint.DamageDiceBonus, Blueprint.DamageType, Blueprint.DamageNoun);
        sb.AppendFormatLine("Hitpoints: {0}d{1}+{2}", Blueprint.HitPointDiceCount, Blueprint.HitPointDiceValue, Blueprint.HitPointDiceBonus);
        sb.AppendFormatLine("Mana: {0}d{1}+{2}", Blueprint.ManaDiceCount, Blueprint.ManaDiceValue, Blueprint.ManaDiceBonus);
        sb.AppendFormatLine("Hit roll: {0}", Blueprint.HitRollBonus);
        sb.AppendFormatLine("Bash: {0} Pierce: {1} Slash: {2} Exotic: {3}", Blueprint.ArmorBash, Blueprint.ArmorPierce, Blueprint.ArmorSlash, Blueprint.ArmorExotic);
        sb.AppendFormatLine("Flags: {0}", Blueprint.CharacterFlags);
        sb.AppendFormatLine("Offensive: {0}", Blueprint.OffensiveFlags);
        sb.AppendFormatLine("Act: {0}", Blueprint.ActFlags);
        sb.AppendFormatLine("Immunities: {0}", Blueprint.Immunities);
        sb.AppendFormatLine("Resistances: {0}", Blueprint.Resistances);
        sb.AppendFormatLine("Vulnerabilities: {0}", Blueprint.Vulnerabilities);
        sb.AppendFormatLine("Shields: {0}", Blueprint.ShieldFlags);
        sb.AppendFormatLine("StartPos: {0} DefaultPos: {1}", Blueprint.StartPosition, Blueprint.DefaultPosition);
        sb.AppendFormatLine("Specials: {0}", Blueprint.SpecialBehavior);
        // TODO: loot table, script
        // TODO: specific Blueprint
        switch (Blueprint)
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
            case CharacterPetShopBlueprint characterPetShopBlueprint:
                sb.AppendLine("Pet shopkeeper:");
                sb.AppendFormatLine("Pets: {0}", string.Join(",", characterPetShopBlueprint.PetBlueprints.Select(x => $"{x.Name} (id: {x.Id})")));
                sb.AppendFormatLine("Profit buy: {0}% sell: {1}%", characterPetShopBlueprint.ProfitBuy, characterPetShopBlueprint.ProfitSell);
                sb.AppendFormatLine("Open hour: {0} Close hour: {1}", characterPetShopBlueprint.OpenHour, characterPetShopBlueprint.CloseHour);
                break;
        }
        Actor.Send(sb);
    }
}
