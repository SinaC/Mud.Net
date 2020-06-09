using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public class AbilityActionInput : ActionInput
    {
        public AbilityInfo AbilityInfo => Context as AbilityInfo;
        public int? Level { get; }

        public AbilityActionInput(AbilityInfo abilityInfo, IActor actor, string rawParameters, params CommandParameter[] parameters)
            : base(actor, string.Empty /*TODO*/, abilityInfo?.Name ?? "Error", rawParameters, parameters)
        {
            Context = abilityInfo;
            Level = null;
        }

        public AbilityActionInput(AbilityInfo abilityInfo, IActor actor, int level, string rawParameters, params CommandParameter[] parameters)
            : base(actor, string.Empty /*TODO*/, abilityInfo?.Name ?? "Error", rawParameters, parameters)
        {
            Context = abilityInfo;
            Level = level;
        }
    }
}
