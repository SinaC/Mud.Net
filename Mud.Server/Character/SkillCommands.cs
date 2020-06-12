using Mud.Domain;
using Mud.Logger;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Input;
using Mud.Server.Interfaces.Ability;
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

        [CharacterCommand("recite", "Skills", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <scroll> [<target>]")]
        protected virtual CommandExecutionResults DoRecite(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Scrolls", rawParameters, parameters);

        [CharacterCommand("zap", "Skills", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <wand> [<target>]")]
        protected virtual CommandExecutionResults DoZap(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Wands", rawParameters, parameters);

        [CharacterCommand("brandish", "Skills", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <staff> [<target>]")]
        protected virtual CommandExecutionResults DoBrandish(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Staves", rawParameters, parameters);

        [CharacterCommand("rescue", "Skills", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <victim>")]
        protected virtual CommandExecutionResults DoRescue(string rawParameters, params CommandParameter[] parameters) => ExecuteSkill("Rescue", rawParameters, parameters);

        //
        private CommandExecutionResults ExecuteSkill(string abilityName, string rawParameters, params CommandParameter[] parameters)
        {
            var abilityInfo = AbilityManager[abilityName];
            if (abilityInfo == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Skill {0} not found.", abilityName);
                Send("Something goes wrong.");
                return CommandExecutionResults.Error;
            }
            var skillInstance = AbilityManager.CreateInstance<ISkill>(abilityName);
            if (skillInstance == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Skill {0} cannot be created.", abilityName);
                Send("Something goes wrong.");
                return CommandExecutionResults.Error;
            }
            var skillActionInput = new SkillActionInput(abilityInfo, this, rawParameters, parameters);
            string skillInstanceGuards = skillInstance.Setup(skillActionInput);
            if (skillInstanceGuards != null)
            {
                Send(skillInstanceGuards);
                return CommandExecutionResults.NoExecution;
            }
            skillInstance.Execute();
            return CommandExecutionResults.Ok;
        }
    }
}
