using Mud.Server.Ability.Interfaces;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability.Spell;

// TODO: move this in a specific project
public class SpellActionInput : ISpellActionInput
{
    public ICharacter Caster { get; }
    public ICommandParameter[] Parameters { get; }
    public IAbilityDefinition AbilityDefinition { get; }
    public int Level { get; }
    public CastFromItemOptions CastFromItemOptions { get; } = default!;
    public bool IsCastFromItem => CastFromItemOptions != null;

    public SpellActionInput(IAbilityDefinition abilityDefinition, ICharacter caster, int level, params ICommandParameter[] parameters)
    {
        Caster = caster;
        Parameters = parameters;
        AbilityDefinition = abilityDefinition;
        Level = level;
    }

    public SpellActionInput(IAbilityDefinition abilityDefinition, ICharacter caster, int level, CastFromItemOptions castFromItemOptions, params ICommandParameter[] parameters)
    {
        Caster = caster;
        Parameters = parameters;
        AbilityDefinition = abilityDefinition;
        Level = level;
        CastFromItemOptions = castFromItemOptions;
    }
}
