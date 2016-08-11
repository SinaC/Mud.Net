using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Server.Abilities;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("use", Category = "Ability", Priority = 2)]
        [Command("cast", Category = "Ability", Priority = 2)]
        protected virtual bool DoCast(string rawParameters, params CommandParameter[] parameters)
        {
            Repository.AbilityManager.Process(this, parameters);
            return true;
        }

        [Command("spells", Category = "Ability")]
        [Command("skills", Category = "Ability")]
        [Command("abilities", Category = "Ability")]
        protected virtual bool DoAbilities(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayAll = parameters.Length > 0 && parameters[0].IsAll; // Display spells below level or all ?
            // TODO: color
            // TODO: split into spells/skills

            List<AbilityAndLevel> abilities = KnownAbilities
                .Where(x => (x.Ability.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed && (displayAll || x.Level <= Level))
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Ability.Name)
                .ToList();
            // Test purpose: every abilities
            //List<AbilityAndLevel> abilities = Repository.AbilityManager.Abilities
            //    .Where(x => (x.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed)
            //    .Select(x => new AbilityAndLevel(1, x))
            //    .OrderBy(x => x.Ability.Name)
            //    .ToList();

            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine("+-------------------------------------------------+");
            //sb.AppendLine("| Abilities                                       |");
            //sb.AppendLine("+-----+-----------------------+----------+--------+");
            //sb.AppendLine("| Lvl | Name                  | Resource | Cost   |");
            //sb.AppendLine("+-----+-----------------------+----------+--------+");
            //foreach (AbilityAndLevel abilityAndLevel in abilities)
            //{
            //    int level = abilityAndLevel.Level;
            //    IAbility ability = abilityAndLevel.Ability;
            //    if ((ability.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
            //        sb.AppendFormatLine("| {0,3} | {1,21} |  %m%passive ability%x%  |", level, ability.Name);
            //    else if (ability.CostType == AmountOperators.Percentage)
            //        sb.AppendFormatLine("| {0,3} | {1,21} | {2,14} | {3,5}% |", level, ability.Name, StringHelpers.ResourceColor(ability.ResourceKind), ability.CostAmount);
            //    else if (ability.CostType == AmountOperators.Fixed)
            //        sb.AppendFormatLine("| {0,3} | {1,21} | {2,14} | {3,6} |", level, ability.Name, StringHelpers.ResourceColor(ability.ResourceKind), ability.CostAmount);
            //    else
            //        sb.AppendFormatLine("| {0,3} | {1,21} | %W%free cost ability%x% |", level, ability.Name);
            //}
            //sb.AppendLine("+-----+-----------------------+----------+--------+");
            //Page(sb);

            
            StringBuilder sb2 = AbilitiesAndLevelTableTableGenerator.Value.Generate(abilities);
            Page(sb2);

            return true;
        }

        private static readonly Lazy<TableGenerator<AbilityAndLevel>> AbilitiesAndLevelTableTableGenerator = new Lazy<TableGenerator<AbilityAndLevel>>(() => GenerateAbilitiesAndLevelTableGenerator);

        private static TableGenerator<AbilityAndLevel> GenerateAbilitiesAndLevelTableGenerator
        {
            get
            {
                TableGenerator<AbilityAndLevel> generator = new TableGenerator<AbilityAndLevel>("Abilities");
                generator.AddColumn("Lvl", 5, x => x.Level.ToString());
                generator.AddColumn("Name", 23, x => x.Ability.Name);
                generator.AddColumn("Resource", 10,
                    x =>
                    {
                        if ((x.Ability.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
                            return "%m%passive ability%x%";
                        if (x.Ability.CostType == AmountOperators.Percentage || x.Ability.CostType == AmountOperators.Fixed)
                            return StringHelpers.ResourceColor(x.Ability.ResourceKind);
                        return "%W%free cost ability%x%";
                    },
                    x =>
                    {
                        if ((x.Ability.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
                            return 1;
                        if (x.Ability.CostType == AmountOperators.Percentage || x.Ability.CostType == AmountOperators.Fixed)
                            return 0;
                        return 1;
                    });
                generator.AddColumn("Cost", 8, x => x.Ability.CostAmount.ToString(), x => x.Ability.CostType == AmountOperators.Percentage ? "% " : " ");
                return generator;
            }
        }
    }
}
