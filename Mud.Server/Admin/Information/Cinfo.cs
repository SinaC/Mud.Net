using Mud.Common;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Quest;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.World;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("cinfo", "Information")]
    [AdminCommand("minfo", "Information")]
    [Syntax("[cmd] <id>")]
    public class Cinfo : AdminGameAction
    {
        private IWorld World { get; }

        public CharacterBlueprintBase Blueprint { get; protected set; }

        public Cinfo(IWorld world)
        {
            World = world;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return BuildCommandSyntax();

            if (!actionInput.Parameters[0].IsNumber)
                return BuildCommandSyntax();

            int id = actionInput.Parameters[0].AsNumber;
            Blueprint = World.GetCharacterBlueprint(id);
            if (Blueprint == null)
                return "Not found.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormatLine("Id: {0} Type: {1}", Blueprint.Id, Blueprint.GetType());
            sb.AppendFormatLine("Name: {0}", Blueprint.Name);
            sb.AppendFormatLine("ShortDescription: {0}", Blueprint.ShortDescription);
            sb.AppendFormatLine("LongDescription: {0}", Blueprint.LongDescription);
            sb.AppendFormatLine("Description: {0}", Blueprint.Description);
            sb.AppendFormatLine("Level: {0} Sex: {1}", Blueprint.Level, Blueprint.Sex);
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
            }
            Actor.Send(sb);
        }
    }
}
