using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Interfaces.Ability;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [CharacterCommand("cast", "Ability", Priority = 2, MinPosition = Positions.Fighting)]
        [Syntax("[cmd] <ability> <target>")]
        protected virtual CommandExecutionResults DoCast(string rawParameters, params CommandParameter[] parameters)
        {
            CastResults result = AbilityManager.Cast(this, rawParameters, parameters);
            return result == CastResults.Ok // TODO;: better mapping
                ? CommandExecutionResults.Ok
                : CommandExecutionResults.NoExecution;
        }

        [CharacterCommand("abilities", "Ability")]
        [Syntax(
            "[cmd]",
            "[cmd] all")]
        protected virtual CommandExecutionResults DoAbilities(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayAll = parameters.Length > 0 && parameters[0].IsAll; // Display abilities below level or all ?

            DisplayAbilitiesList(displayAll, x => true);

            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("spells", "Ability")]
        [Syntax(
            "[cmd]",
            "[cmd] all")]
        protected virtual CommandExecutionResults DoSpells(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayAll = parameters.Length > 0 && parameters[0].IsAll; // Display spells below level or all ?

            DisplayAbilitiesList(displayAll, x => x == AbilityKinds.Spell);

            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("skills", "Ability")]
        [Syntax(
            "[cmd]",
            "[cmd] all")]
        protected virtual CommandExecutionResults DoSkills(string rawParameters, params CommandParameter[] parameters)
        {
            bool displayAll = parameters.Length > 0 && parameters[0].IsAll; // Display skills+passive below level or all ?

            DisplayAbilitiesList(displayAll, x => x == AbilityKinds.Skill || x == AbilityKinds.Passive);

            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("cd", "Ability")]
        [CharacterCommand("cooldowns", "Ability")]
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
                        .Select(x => new { Ability = x.Key, SecondsLeft = x.Value / Pulse.PulsePerSeconds})
                        .OrderBy(x => x.SecondsLeft))
                    {
                        sb.AppendFormatLine("%B%{0}%x% is in cooldown for %W%{1}%x%.", cooldown.Ability.Name, StringHelpers.FormatDelay(cooldown.SecondsLeft));
                    }
                    Send(sb);
                    return CommandExecutionResults.Ok;
                }
                Send("%c%No abilities in cooldown.%x%");
                return CommandExecutionResults.Ok;
            }
            //
            IKnownAbility knownAbility = AbilityManager.Search(KnownAbilities, Level, _ => true, parameters[0]);
            if (knownAbility == null)
            {
                Send("You don't know any abilities of that name.");
                return CommandExecutionResults.InvalidTarget;
            }
            int pulseLeft = CooldownPulseLeft(knownAbility.Ability);
            if (pulseLeft <= 0)
                Send("{0} is not in cooldown.", knownAbility.Ability.Name);
            else
                Send("{0} is in cooldown for {1}.", knownAbility.Ability.Name, StringHelpers.FormatDelay(pulseLeft/Pulse.PulsePerSeconds));
            return CommandExecutionResults.Ok;
        }

        #region Helpers

        private void DisplayAbilitiesList(bool displayAll, Func<AbilityKinds, bool> filterOnAbilityKind) 
        {
            IEnumerable<IKnownAbility> abilities = KnownAbilities
                //.Where(x => (displayAll || x.Level <= Level) && (displayAll || x.Learned > 0) && filterOnAbilityKind(x.Ability.Kind))
                .Where(x => (displayAll || x.Level <= Level) && filterOnAbilityKind(x.Ability.Kind))
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Ability.Name);

            StringBuilder sb = TableGenerators.KnownAbilityTableGenerator.Value.Generate("Abilities", abilities);
            Page(sb);
        }

        #endregion
    }
}
