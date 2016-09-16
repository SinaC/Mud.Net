using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Server.Abilities;
using Mud.Server.Common;
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

            //+------------------------------------------------------------+
            //| Abilities                                                  |
            //+-----+-----------------------+----------+--------+----------+
            //| Lvl | Name                  | Resource | Cost   | Cooldown |
            //+-----+-----------------------+----------+--------+----------+

            StringBuilder sb = AbilitiesAndLevelTableGeneratorInstance.Value.Generate(abilities);
            Page(sb);

            return true;
        }

        [Command("cd", Category = "Ability")]
        [Command("cooldowns", Category = "Ability")]
        protected virtual bool DoCooldowns(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                //IReadOnlyCollection<KeyValuePair<IAbility, DateTime>> abilitiesInCooldown = AbilitiesInCooldown;
                if (AbilitiesInCooldown.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("%c%Following abilities are in cooldown:%x%");
                    foreach (var cooldown in AbilitiesInCooldown
                        .Select(x => new { Ability = x.Key, SecondsLeft = (x.Value - Repository.Server.CurrentTime).TotalSeconds })
                        .OrderBy(x => x.SecondsLeft))
                    {
                        int secondsLeft = (int)Math.Ceiling(cooldown.SecondsLeft);
                        sb.AppendFormatLine("{0} is in cooldown for {1}.", cooldown.Ability.Name, StringHelpers.FormatDelay(secondsLeft));
                    }
                    Send(sb);
                }
                else
                    Send("%c%No abilities in cooldown.%x%");
            }
            else
            {
                IAbility ability = Repository.AbilityManager.Search(parameters[0]);
                if (ability == null)
                {
                    Send("You don't know any abilities of that name.");
                    return true;
                }
                int cooldownSecondsLeft = CooldownSecondsLeft(ability);
                if (cooldownSecondsLeft <= 0)
                    Send("{0} is not in cooldown.", ability.Name);
                else
                    Send("{0} is in cooldown for {1}.", ability.Name, StringHelpers.FormatDelay(cooldownSecondsLeft));
            }
            return true;
        }

        #region Helpers

        private static readonly Lazy<TableGenerator<AbilityAndLevel>> AbilitiesAndLevelTableGeneratorInstance = new Lazy<TableGenerator<AbilityAndLevel>>(() => AbilitiesAndLevelTableGenerator);

        private static TableGenerator<AbilityAndLevel> AbilitiesAndLevelTableGenerator
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
                generator.AddColumn("Cooldown", 10, x => x.Ability.Cooldown > 0 ? StringHelpers.FormatDelayShort(x.Ability.Cooldown) : "---");
                return generator;
            }
        }

        #endregion
    }
}
