using Mud.Server.Input;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Ability.Spell
{
    // TODO: move this in a specific project
    public class SpellActionInput : ISpellActionInput
    {
        public ICharacter Caster { get; }
        public string RawParameters { get; }
        public CommandParameter[] Parameters { get; }
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

        public SpellActionInput(IAbilityInfo abilityInfo, ICharacter caster, int level, CastFromItemOptions castFromItemOptions, string rawParameters, params CommandParameter[] parameters)
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
