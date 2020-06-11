using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public class SpellActionInput
    {
        public ICharacter Caster { get; }
        public string RawParameters { get; }
        public CommandParameter[] Parameters { get; }
        public AbilityInfo AbilityInfo { get; }
        public int Level { get; }
        public CastFromItemOptions CastFromItemOptions { get; }

        public bool IsCastFromItem => CastFromItemOptions != null;

        public SpellActionInput(ActionInput actionInput, AbilityInfo abilityInfo, ICharacter caster, int level)
        {
            Caster = caster;
            var parameters = CommandHelpers.SkipParameters(actionInput.Parameters, 1); // remove 'spell name'
            RawParameters = parameters.rawParameters;
            Parameters = parameters.parameters;
            AbilityInfo = abilityInfo;
            Level = level;
            CastFromItemOptions = null;
        }

        public SpellActionInput(AbilityInfo abilityInfo, ICharacter caster, int level, CastFromItemOptions castFromItemOptions, string rawParameters, params CommandParameter[] parameters)
        {
            Caster = caster;
            RawParameters = rawParameters;
            Parameters = parameters;
            AbilityInfo = abilityInfo;
            Level = level;
            CastFromItemOptions = castFromItemOptions;
        }
    }

    public class CastFromItemOptions
    {
        public IItem Item { get; set; }
        public IEntity PredefinedTarget { get; set; }
    }
}
