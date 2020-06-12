using Mud.Server.Input;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability
{
    // TODO: move this in a specific project
    public class SkillActionInput : ISkillActionInput
    {
        public ICharacter User { get; }
        public string RawParameters { get; }
        public CommandParameter[] Parameters { get; }
        public IAbilityInfo AbilityInfo { get; }

        public SkillActionInput(IActionInput actionInput, IAbilityInfo abilityInfo, ICharacter user)
        {
            User = user;
            RawParameters = actionInput.RawParameters;
            Parameters = actionInput.Parameters;
            AbilityInfo = abilityInfo;
        }
    }
}
