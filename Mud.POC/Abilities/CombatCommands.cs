using Mud.Logger;
using Mud.Server.Input;

namespace Mud.POC.Abilities
{
    public partial class PlayableCharacter // TODO: should be CharacterBase
    {
        [Command("berserk", "combat", "skills")]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoBerserk(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("berserk", rawParameters, parameters);
        }

        [Command("bash", "combat", "skills")]
        [Syntax("[cmd] <victim>")]
        protected virtual CommandExecutionResults DoBash(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("bash", rawParameters, parameters);
        }

        [Command("dirt", "combat", "skills")]
        [Syntax("[cmd] <victim>")]
        protected virtual CommandExecutionResults DoDirt(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("dirt kicking", rawParameters, parameters);
        }

        [Command("trip", "combat", "skills")]
        [Syntax("[cmd] <victim>")]
        protected virtual CommandExecutionResults DoTrip(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("trip", rawParameters, parameters);
        }

        [Command("backstab", "combat", "skills")]
        [Command("bs", "combat", "skills")]
        [Syntax("[cmd] <victim>")]
        protected virtual CommandExecutionResults DoBackstab(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("backstab", rawParameters, parameters);
        }

        [Command("kick", "combat", "skills")]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoKick(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("kick", rawParameters, parameters);
        }

        [Command("disarm", "combat", "skills")]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoDisarm(string rawParameters, params CommandParameter[] parameters)
        {
            return ExecuteSkill("disarm", rawParameters, parameters);
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
