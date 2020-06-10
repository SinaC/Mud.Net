using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public class SkillActionInput : ActionInput
    {
        public AbilityInfo AbilityInfo => Context as AbilityInfo;

        public SkillActionInput(AbilityInfo abilityInfo, IActor actor, string rawParameters, params CommandParameter[] parameters)
            : base(actor, string.Empty /*TODO*/, abilityInfo?.Name ?? "Error", rawParameters, parameters)
        {
            Context = abilityInfo;
        }
    }
}
