using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Server.Abilities;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.World;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("use", Priority = 2)]
        [Command("cast", Priority = 2)]
        protected virtual bool DoCast(string rawParameters, params CommandParameter[] parameters)
        {
            AbilityManager manager = new AbilityManager();
            manager.Process(this, parameters);
            return true;
        }

        [Command("spells")]
        [Command("skills")]
        [Command("abilities")]
        protected virtual bool DoAbilities(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: color
            // TODO: split into spells/skills
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("+-------------------------------------------+");
            sb.AppendLine("| Abilities                                 |");
            sb.AppendLine("+-----------------------+----------+--------+");
            sb.AppendLine("| Name                  | Resource | Cost   |");
            sb.AppendLine("+-----------------------+----------+--------+");
            AbilityManager manager = new AbilityManager();
            List<IAbility> abilities = manager.Abilities.Where(x => (x.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed).ToList();
            foreach (IAbility ability in abilities)
                if (ability.CostType == AmountOperators.Percentage)
                    sb.AppendFormatLine("| {0,21} | {3}{1,8}%x% | {2,5}% |", ability.Name, ability.ResourceKind, ability.CostAmount, ResourceColor(ability.ResourceKind));
                else if (ability.CostType == AmountOperators.Fixed)
                    sb.AppendFormatLine("| {0,21} | {3}{1,8}%x% | {2,6} |", ability.Name, ability.ResourceKind, ability.CostAmount, ResourceColor(ability.ResourceKind));
                else
                    sb.AppendFormatLine("| {0,21} |  ------  |  ----  |", ability.Name, ability.ResourceKind, ability.CostAmount, ability.CostType == AmountOperators.Percentage ? "%" : " ");
            sb.AppendLine("+-----------------------+----------+--------+");
            Page(sb);
            return true;
        }

        //*********************** Helpers **************************
        public string ResourceColor(ResourceKinds resource)
        {
            switch (resource)
            {
                case ResourceKinds.Mana:
                    return "%B%";
                    case ResourceKinds.Energy:
                    return "%y%";
                    case ResourceKinds.Rage:
                    return "%r%";
                    case ResourceKinds.Runic:
                    return "%c%";
                default:
                    return "%x%";
            }
        }
    }
}
