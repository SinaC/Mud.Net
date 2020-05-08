using Mud.Domain;
using Mud.Logger;
using Mud.Server.Abilities;
using Mud.Server.Input;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [Command("use", "skills")]
        protected virtual CommandExecutionResults DoUse(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: refactor, almost same code in AbilityManager
            if (parameters.Length == 0)
            {
                Send("Use which what where ?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            // searck skill
            KnownAbility knownAbility = AbilityManager.Search(KnownAbilities, Level, x => x.Kind == AbilityKinds.Spell, parameters[0]);
            if (knownAbility == null)
            {
                Send("You don't know any spells of that name.");
                return CommandExecutionResults.InvalidParameter;
            }

            // strip first argument
            (rawParameters, parameters) = CommandHelpers.SkipParameters(parameters, 1);

            // use skill
            UseResults result = AbilityManager.Use(knownAbility.Ability, this, rawParameters, parameters);

            return MapUseResults(result);
        }

        [Command("berserk", "combat", "skills")]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoBerserk(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("Berserk", rawParameters, parameters);
        }

        [Command("bash", "combat", "skills")]
        [Syntax("[cmd] <victim>")]
        protected virtual CommandExecutionResults DoBash(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("Bash", rawParameters, parameters);
        }

        [Command("dirt", "combat", "skills")]
        [Syntax("[cmd] <victim>")]
        protected virtual CommandExecutionResults DoDirt(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("Dirt kicking", rawParameters, parameters);
        }

        [Command("trip", "combat", "skills")]
        [Syntax("[cmd] <victim>")]
        protected virtual CommandExecutionResults DoTrip(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("Trip", rawParameters, parameters);
        }

        [Command("backstab", "combat", "skills")]
        [Command("bs", "combat", "skills")]
        [Syntax("[cmd] <victim>")]
        protected virtual CommandExecutionResults DoBackstab(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("Backstab", rawParameters, parameters);
        }

        [Command("kick", "combat", "skills")]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoKick(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("Kick", rawParameters, parameters);
        }

        [Command("disarm", "combat", "skills")]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoDisarm(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("dSsarm", rawParameters, parameters);
        }

        [Command("sneak", "skills")]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoSneak(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("Sneak", rawParameters, parameters);
        }

        [Command("hide", "skills")]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoHide(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("Hide", rawParameters, parameters);
        }

        //
        private CommandExecutionResults ExecuteSkill(string abilityName, string rawParameters, params CommandParameter[] parameters)
        {
            IAbility ability = AbilityManager[abilityName];
            if (ability == null)
                return CommandExecutionResults.InvalidParameter;
            UseResults result = AbilityManager.Use(ability, this, rawParameters, parameters);
            return MapUseResults(result);
        }

        private CommandExecutionResults MapUseResults(UseResults result)
        {
            switch (result)
            {
                case UseResults.Ok: return CommandExecutionResults.Ok;
                case UseResults.MissingParameter: return CommandExecutionResults.SyntaxErrorNoDisplay;
                case UseResults.InvalidParameter: return CommandExecutionResults.InvalidParameter;
                case UseResults.InvalidTarget: return CommandExecutionResults.InvalidTarget;
                case UseResults.TargetNotFound: return CommandExecutionResults.TargetNotFound;
                case UseResults.CantUseRequiredResource: return CommandExecutionResults.NoExecution;
                case UseResults.NotEnoughResource: return CommandExecutionResults.NoExecution;
                case UseResults.Failed: return CommandExecutionResults.NoExecution;
                case UseResults.NotKnown: return CommandExecutionResults.NoExecution;
                case UseResults.MustBeFighting: return CommandExecutionResults.NoExecution;
                case UseResults.Error: return CommandExecutionResults.Error;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unexpected UseResults {0}", result);
                    return CommandExecutionResults.Error;
            }
        }
    }
}
