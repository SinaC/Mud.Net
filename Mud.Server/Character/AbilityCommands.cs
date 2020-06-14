using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Server.Abilities;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [Command("use", "Ability", Priority = 2)]
        [Command("cast", "Ability", Priority = 2)]
        [Syntax("[cmd] <ability> <target>")]
        protected virtual CommandExecutionResults DoCast(string rawParameters, params CommandParameter[] parameters)
        {
            bool processed = AbilityManager.Process(this, parameters); // TODO: change Process return type to CommandExecutionResults?
            return processed
                ? CommandExecutionResults.Ok
                : CommandExecutionResults.NoExecution;
        }

        [Command("abilities", "Ability")]
        [Syntax(
            "[cmd]",
            "[cmd] all")]
        protected virtual CommandExecutionResults DoAbilities(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayAll = parameters.Length > 0 && parameters[0].IsAll; // Display spells below level or all ?

            DisplayAbilitiesList(displayAll, x => true);

            return CommandExecutionResults.Ok;
        }

        [Command("spells", "Ability")]
        [Syntax(
            "[cmd]",
            "[cmd] all")]
        protected virtual CommandExecutionResults DoSpells(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayAll = parameters.Length > 0 && parameters[0].IsAll; // Display spells below level or all ?

            DisplayAbilitiesList(displayAll, x => x == AbilityKinds.Spell);

            return CommandExecutionResults.Ok;
        }

        [Command("skills", "Ability")]
        [Syntax(
            "[cmd]",
            "[cmd] all")]
        protected virtual CommandExecutionResults DoSkills(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayAll = parameters.Length > 0 && parameters[0].IsAll; // Display spells below level or all ?

            DisplayAbilitiesList(displayAll, x => x == AbilityKinds.Skill);

            return CommandExecutionResults.Ok;
        }

        [Command("cd", "Ability")]
        [Command("cooldowns", "Ability")]
        [Syntax(
            "[cmd]",
            "[cmd] <ability>")]
        protected virtual CommandExecutionResults DoCooldowns(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (AbilitiesInCooldown.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("%c%Following abilities are in cooldown:%x%");
                    foreach (var cooldown in AbilitiesInCooldown
                        .Select(x => new { Ability = x.Key, SecondsLeft = (x.Value - TimeHandler.CurrentTime).TotalSeconds })
                        .OrderBy(x => x.SecondsLeft))
                    {
                        int secondsLeft = (int)Math.Ceiling(cooldown.SecondsLeft);
                        sb.AppendFormatLine("{0} is in cooldown for {1}.", cooldown.Ability.Name, StringHelpers.FormatDelay(secondsLeft));
                    }
                    Send(sb);
                    return CommandExecutionResults.Ok;
                }
                Send("%c%No abilities in cooldown.%x%");
                return CommandExecutionResults.Ok;
            }
            //
            IAbility ability = AbilityManager.Search(parameters[0]);
            if (ability == null)
            {
                Send("You don't know any abilities of that name.");
                return CommandExecutionResults.InvalidTarget;
            }
            int cooldownSecondsLeft = CooldownSecondsLeft(ability);
            if (cooldownSecondsLeft <= 0)
                Send("{0} is not in cooldown.", ability.Name);
            else
                Send("{0} is in cooldown for {1}.", ability.Name, StringHelpers.FormatDelay(cooldownSecondsLeft));
            return CommandExecutionResults.Ok;
        }

        #region Helpers

        private void DisplayAbilitiesList(bool displayAll, Func<AbilityKinds, bool> filterOnAbilityKind) 
        {
            IEnumerable<AbilityAndLevel> abilities = KnownAbilities
                .Where(x => (x.Ability.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed && (displayAll || x.Level <= Level) && filterOnAbilityKind(x.Ability.Kind))
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Ability.Name);

            StringBuilder sb = TableGenerators.AbilityAndLevelTableGenerator.Value.Generate("Abilities", abilities);
            Page(sb);
        }

        #endregion
    }
}
