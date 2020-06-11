using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public class SkillActionInput
    {
        public ICharacter User { get; }
        public string RawParameters { get; }
        public CommandParameter[] Parameters { get; }
        public AbilityInfo AbilityInfo { get; }

        public SkillActionInput(ActionInput actionInput, AbilityInfo abilityInfo, ICharacter user)
        {
            User = user;
            RawParameters = actionInput.RawParameters;
            Parameters = actionInput.Parameters;
            AbilityInfo = abilityInfo;
        }

        //public SkillActionInput(AbilityInfo abilityInfo, ICharacter user, string rawParameters, params CommandParameter[] parameters)
        //{
        //    User = user;
        //    RawParameters = rawParameters;
        //    Parameters = parameters;
        //    AbilityInfo = abilityInfo;
        //}
    }
}
