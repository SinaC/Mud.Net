using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public class AbilityActionInput : ActionInput
    {
        public AbilityInfo AbilityInfo => Context as AbilityInfo;

        public AbilityActionInput(AbilityInfo abilityInfo, IActor actor, string rawParameters, params CommandParameter[] parameters)
            : base(actor, string.Empty /*TODO*/, abilityInfo?.Name ?? "Error", rawParameters, parameters)
        {
            Context = abilityInfo;
        }
    }
}
