using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public class SpellActionInput : ActionInput
    {
        public AbilityInfo AbilityInfo => Context as AbilityInfo;
        public int Level { get; }
        public CastFromItemOptions CastFromItemOptions { get; }

        public bool IsCastFromItem => CastFromItemOptions != null;

        public SpellActionInput(AbilityInfo abilityInfo, IActor actor, int level, bool isCastFromItem, string rawParameters, params CommandParameter[] parameters) // TODO: remove this
            : this(abilityInfo, actor, level, isCastFromItem ? new CastFromItemOptions() : null, rawParameters, parameters)
        {
            Context = abilityInfo;
            Level = level;
        }

        public SpellActionInput(AbilityInfo abilityInfo, IActor actor, int level, CastFromItemOptions castFromItemOptions, string rawParameters, params CommandParameter[] parameters)
            : base(actor, string.Empty /*TODO*/, abilityInfo?.Name ?? "Error", rawParameters, parameters)
        {
            Context = abilityInfo;
            Level = level;
            CastFromItemOptions = castFromItemOptions;
        }
    }

    public class CastFromItemOptions
    {
        public IEntity PredefinedTarget { get; set; }
    }
}
