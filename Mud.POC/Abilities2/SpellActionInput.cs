using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.POC.Abilities2
{
    public class SpellActionInput : ISpellActionInput
    {
        public ICharacter Caster { get; }
        public string RawParameters { get; }
        public ICommandParameter[] Parameters { get; }
        public IAbilityInfo AbilityInfo { get; }
        public int Level { get; }
        public CastFromItemOptions CastFromItemOptions { get; }
        public bool IsCastFromItem => CastFromItemOptions != null;

        public SpellActionInput(IActionInput actionInput, IAbilityInfo abilityInfo, ICharacter caster, int level)
        {
            Caster = caster;
            var parameters = CommandHelpers.SkipParameters(actionInput.Parameters, 1); // remove 'spell name'
            RawParameters = parameters.rawParameters;
            Parameters = parameters.parameters;
            AbilityInfo = abilityInfo;
            Level = level;
            CastFromItemOptions = null;
        }

        public SpellActionInput(IAbilityInfo abilityInfo, ICharacter caster, int level, CastFromItemOptions castFromItemOptions, string rawParameters, params ICommandParameter[] parameters)
        {
            Caster = caster;
            RawParameters = rawParameters;
            Parameters = parameters;
            AbilityInfo = abilityInfo;
            Level = level;
            CastFromItemOptions = castFromItemOptions;
        }
    }
}
