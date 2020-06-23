using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Linq;

namespace Mud.Server.Ability.Spell
{
    // TODO: move this in a specific project
    public class SpellActionInput : ISpellActionInput
    {
        public ICharacter Caster { get; }
        public ICommandParameter[] Parameters { get; }
        public IAbilityInfo AbilityInfo { get; }
        public int Level { get; }
        public CastFromItemOptions CastFromItemOptions { get; }
        public bool IsCastFromItem => CastFromItemOptions != null;

        public SpellActionInput(IActionInput actionInput, IAbilityInfo abilityInfo, ICharacter caster, int level)
        {
            Caster = caster;
            Parameters = actionInput.Parameters.Skip(1).ToArray();
            AbilityInfo = abilityInfo;
            Level = level;
            CastFromItemOptions = null;
        }

        public SpellActionInput(IAbilityInfo abilityInfo, ICharacter caster, int level, CastFromItemOptions castFromItemOptions, params ICommandParameter[] parameters)
        {
            Caster = caster;
            Parameters = parameters;
            AbilityInfo = abilityInfo;
            Level = level;
            CastFromItemOptions = castFromItemOptions;
        }
    }
}
