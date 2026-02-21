using Mud.Server.Ability.Interfaces;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.Parser.Interfaces;
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
    public bool SaySpell { get; }

    public SpellActionInput(IAbilityDefinition abilityDefinition, ICharacter caster, int level, bool saySpell, params ICommandParameter[] parameters)
    {
        Caster = caster;
        Parameters = parameters;
        AbilityDefinition = abilityDefinition;
        Level = level;
        SaySpell = saySpell;
    }

    public SpellActionInput(IAbilityDefinition abilityDefinition, ICharacter caster, int level, CastFromItemOptions castFromItemOptions, params ICommandParameter[] parameters)
    {
        Caster = caster;
        Parameters = parameters;
        AbilityDefinition = abilityDefinition;
        Level = level;
        CastFromItemOptions = castFromItemOptions;
        SaySpell = false;
    }
}
