using Mud.Domain;
using Mud.Logger;
using Mud.Server.Input;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [CharacterCommand("berserk", "Combat", "Skills", MinPosition = Positions.Fighting)]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoBerserk(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Berserk", rawParameters, parameters);

        [CharacterCommand("bash", "Combat", "Skills", MinPosition = Positions.Fighting)]
        [Syntax("[cmd] <victim>")]
        protected virtual CommandExecutionResults DoBash(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Bash", rawParameters, parameters);

        [CharacterCommand("dirt", "Combat", "Skills", MinPosition = Positions.Fighting)]
        [Syntax("[cmd] <victim>")]
        protected virtual CommandExecutionResults DoDirt(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Dirt kicking", rawParameters, parameters);

        [CharacterCommand("trip", "Combat", "Skills", MinPosition = Positions.Fighting)]
        [Syntax("[cmd] <victim>")]
        protected virtual CommandExecutionResults DoTrip(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Trip", rawParameters, parameters);

        [CharacterCommand("backstab", "Combat", "Skills", MinPosition = Positions.Fighting)]
        [CharacterCommand("bs", "Combat", "Skills")]
        [Syntax("[cmd] <victim>")]
        protected virtual CommandExecutionResults DoBackstab(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Backstab", rawParameters, parameters);

        [CharacterCommand("kick", "Combat", "Skills", MinPosition = Positions.Fighting)]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoKick(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Kick", rawParameters, parameters);

        [CharacterCommand("disarm", "Combat", "Skills", MinPosition = Positions.Fighting)]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoDisarm(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Disarm", rawParameters, parameters);

        [CharacterCommand("sneak", "Skills", MinPosition = Positions.Standing)]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoSneak(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Sneak", rawParameters, parameters);

        [CharacterCommand("hide", "Skills", MinPosition = Positions.Resting)]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoHide(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Hide", rawParameters, parameters);

        [CharacterCommand("recall", "Skills", MinPosition = Positions.Fighting)]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoRecall(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Recall", rawParameters, parameters);

        [CharacterCommand("pick", "Skills", MinPosition = Positions.Resting)]
        [Syntax(
            "[cmd] <direction>",
            "[cmd] <door>",
            "[cmd] <container>|<portal>")]
        protected virtual CommandExecutionResults DoPick(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Pick lock", rawParameters, parameters);

        [CharacterCommand("envenom", "Skills", MinPosition = Positions.Resting)]
        [Syntax(
            "[cmd] <weapon>",
            "[cmd] <food>",
            "[cmd] <drink container>")]
        protected virtual CommandExecutionResults DoEnvenom(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Envenom", rawParameters, parameters);

        [CharacterCommand("recite", "Spells", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <scroll> [<target>]")]
        protected virtual CommandExecutionResults DoRecite(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Scrolls", rawParameters, parameters);

        [CharacterCommand("zap", "Spells", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <wand> [<target>]")]
        protected virtual CommandExecutionResults DoZap(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Wands", rawParameters, parameters);

        [CharacterCommand("brandish", "Spells", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <staff> [<target>]")]
        protected virtual CommandExecutionResults DoBrandish(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Staves", rawParameters, parameters);

        //
        private CommandExecutionResults ExecuteSkill(string abilityName, string rawParameters, params CommandParameter[] parameters)
        {
            IAbility ability = AbilityManager[abilityName];
            if (ability == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "ExecuteSkill: invalid skill {0}", abilityName);
                return CommandExecutionResults.InvalidParameter;
            }

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
