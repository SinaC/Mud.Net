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
        [Command("use", Priority = 2)]
        [Command("cast", Priority = 2)]
        protected virtual bool DoCast(string rawParameters, params CommandParameter[] parameters)
        {
            Repository.AbilityManager.Process(this, parameters);
            return true;
        }

        [Command("spells")]
        [Command("skills")]
        [Command("abilities")]
        protected virtual bool DoAbilities(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayAll = parameters.Length > 0 && parameters[0].Value == "all"; // Display spells below level or all ?
            // TODO: color
            // TODO: split into spells/skills
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("+-------------------------------------------------+");
            sb.AppendLine("| Abilities                                       |");
            sb.AppendLine("+-----+-----------------------+----------+--------+");
            sb.AppendLine("| Lvl | Name                  | Resource | Cost   |");
            sb.AppendLine("+-----+-----------------------+----------+--------+");
            List<AbilityAndLevel> abilities = KnownAbilities
                .Where(x => (x.Ability.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed && (displayAll || x.Level <= Level))
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Ability.Name)
                .ToList();
            //List<AbilityAndLevel> abilities = Repository.AbilityManager.Abilities
            //    .Where(x => (x.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed)
            //    .Select(x => new AbilityAndLevel(1, x))
            //    .OrderBy(x => x)
            //    .ToList();
            foreach (AbilityAndLevel abilityAndLevel in abilities)
            {
                int level = abilityAndLevel.Level;
                IAbility ability = abilityAndLevel.Ability;
                if ((ability.Flags & AbilityFlags.Passive) == AbilityFlags.Passive)
                    sb.AppendFormatLine("| {0,3} | {1,21} |  %m%passive ability%x%  |", level, ability.Name, StringHelpers.ResourceColor(ability.ResourceKind), ability.CostAmount);
                else if (ability.CostType == AmountOperators.Percentage)
                    sb.AppendFormatLine("| {0,3} | {1,21} | {2,14} | {3,5}% |", level, ability.Name, StringHelpers.ResourceColor(ability.ResourceKind), ability.CostAmount);
                else if (ability.CostType == AmountOperators.Fixed)
                    sb.AppendFormatLine("| {0,3} | {1,21} | {2,14} | {3,6} |", level, ability.Name, StringHelpers.ResourceColor(ability.ResourceKind), ability.CostAmount);
                else
                    sb.AppendFormatLine("| {0,3} | {1,21} | %W%free cost ability%x% |", level, ability.Name, ability.ResourceKind, ability.CostAmount, ability.CostType == AmountOperators.Percentage ? "%" : " ");
            }
            sb.AppendLine("+-----+-----------------------+----------+--------+");
            Page(sb);
            return true;
        }
    }
}
