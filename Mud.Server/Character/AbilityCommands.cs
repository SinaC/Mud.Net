using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [CharacterCommand("cast", "Ability", Priority = 2, MinPosition = Positions.Fighting)]
        [Syntax("[cmd] <ability> <target>")]
        protected virtual CommandExecutionResults DoCast(string rawParameters, params ICommandParameter[] parameters)
        {
            //CastResults result = AbilityManager.Cast(this, rawParameters, parameters);
            //return result == CastResults.Ok // TODO;: better mapping
            //    ? CommandExecutionResults.Ok
            //    : CommandExecutionResults.NoExecution;

            // TODO: refactor when new command system will be implemented (see POC.Abilities2.Cast)
            var spellName = parameters.Length > 0 ? parameters[0].Value : null;
            if (string.IsNullOrWhiteSpace(spellName))
            {
                Send("Cast what ?");
                return CommandExecutionResults.SyntaxError;
            }
            var abilityInfo = AbilityManager.Search(spellName, AbilityTypes.Spell);
            if (abilityInfo == null)
            {
                Send("This spell doesn't exist.");
                return CommandExecutionResults.InvalidTarget;
            }
            var abilityLearned = GetAbilityLearned(abilityInfo.Name);
            if (abilityLearned == null)
            {
                Send("You don't know any spells of that name.");
                return CommandExecutionResults.TargetNotFound;
            }
            ISpell spellInstance = AbilityManager.CreateInstance<ISpell>(abilityInfo.Name);
            if (spellInstance == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Spell {0} cannot be created.", abilityInfo.Name);
                Send("Something goes wrong.");
                return CommandExecutionResults.Error;
            }
            var newParameters = CommandHelpers.SkipParameters(parameters, 1);
            var spellActionInput = new SpellActionInput(abilityInfo, this, Level, null, newParameters.rawParameters, newParameters.parameters);
            string spellInstanceGuards = spellInstance.Setup(spellActionInput);
            if (spellInstanceGuards != null)
            {
                Send(spellInstanceGuards);
                return CommandExecutionResults.NoExecution;
            }
            spellInstance.Execute();
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("abilities", "Ability")]
        [Syntax(
            "[cmd]",
            "[cmd] all")]
        protected virtual CommandExecutionResults DoAbilities(string rawParameters, params ICommandParameter[] parameters)
        {
            bool displayAll = parameters.Length > 0 && parameters[0].IsAll; // Display abilities below level or all ?

            DisplayAbilitiesList(displayAll, x => true);

            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("spells", "Ability")]
        [Syntax(
            "[cmd]",
            "[cmd] all")]
        protected virtual CommandExecutionResults DoSpells(string rawParameters, params ICommandParameter[] parameters)
        {
            bool displayAll = parameters.Length > 0 && parameters[0].IsAll; // Display spells below level or all ?

            DisplayAbilitiesList(displayAll, x => x == AbilityTypes.Spell);

            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("skills", "Ability")]
        [Syntax(
            "[cmd]",
            "[cmd] all")]
        protected virtual CommandExecutionResults DoSkills(string rawParameters, params ICommandParameter[] parameters)
        {
            bool displayAll = parameters.Length > 0 && parameters[0].IsAll; // Display skills+passive below level or all ?

            DisplayAbilitiesList(displayAll, x => x == AbilityTypes.Skill || x == AbilityTypes.Passive);

            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("cd", "Ability")]
        [CharacterCommand("cooldowns", "Ability")]
        [Syntax(
            "[cmd]",
            "[cmd] <ability>")]
        protected virtual CommandExecutionResults DoCooldowns(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (AbilitiesInCooldown.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("%c%Following abilities are in cooldown:%x%");
                    foreach (var cooldown in AbilitiesInCooldown
                        .Select(x => new { AbilityName = x.Key, SecondsLeft = x.Value / Pulse.PulsePerSeconds})
                        .OrderBy(x => x.SecondsLeft))
                    {
                        sb.AppendFormatLine("%B%{0}%x% is in cooldown for %W%{1}%x%.", cooldown.AbilityName, cooldown.SecondsLeft.FormatDelay());
                    }
                    Send(sb);
                    return CommandExecutionResults.Ok;
                }
                Send("%c%No abilities in cooldown.%x%");
                return CommandExecutionResults.Ok;
            }
            //
            IAbilityLearned learnedAbility = LearnedAbilities.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameters[0].Value));
            if (learnedAbility == null)
            {
                Send("You don't know any abilities of that name.");
                return CommandExecutionResults.InvalidTarget;
            }
            int pulseLeft = CooldownPulseLeft(learnedAbility.Name);
            if (pulseLeft <= 0)
                Send("{0} is not in cooldown.", learnedAbility.Name);
            else
                Send("{0} is in cooldown for {1}.", learnedAbility.Name, (pulseLeft/Pulse.PulsePerSeconds).FormatDelay());
            return CommandExecutionResults.Ok;
        }

        #region Helpers

        private void DisplayAbilitiesList(bool displayAll, Func<AbilityTypes, bool> filterOnAbilityKind) 
        {
            IEnumerable<IAbilityLearned> abilities = LearnedAbilities
                //.Where(x => (displayAll || x.Level <= Level) && (displayAll || x.Learned > 0) && filterOnAbilityKind(x.Ability.Kind))
                .Where(x => (displayAll || x.Level <= Level) && filterOnAbilityKind(x.AbilityInfo.Type))
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Name);

            StringBuilder sb = TableGenerators.LearnedAbilitiesTableGenerator.Value.Generate("Abilities", abilities);
            Page(sb);
        }

        #endregion
    }
}
