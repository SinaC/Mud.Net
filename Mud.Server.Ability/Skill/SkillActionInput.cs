using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Ability.Skill
{
    public class SkillActionInput : ISkillActionInput
    {
        public ICharacter User { get; }
        public string RawParameters { get; }
        public ICommandParameter[] Parameters { get; }
        public IAbilityInfo AbilityInfo { get; }

        public SkillActionInput(IActionInput actionInput, IAbilityInfo abilityInfo, ICharacter user)
        {
            User = user;
            RawParameters = actionInput.RawParameters;
            Parameters = actionInput.Parameters;
            AbilityInfo = abilityInfo;
        }

        public SkillActionInput(IAbilityInfo abilityInfo, ICharacter user, string rawParameters, params ICommandParameter[] parameters)
        {
            User = user;
            RawParameters = rawParameters;
            Parameters = parameters;
            AbilityInfo = abilityInfo;
        }
    }
}
