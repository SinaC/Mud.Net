using Mud.Server.Ability.Interfaces;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Ability.Spell.Interfaces;

public interface ISpellActionInput
{
    ICharacter Caster { get; }
    ICommandParameter[] Parameters { get; }
    IAbilityDefinition AbilityDefinition { get; }
    int Level { get; }
    CastFromItemOptions CastFromItemOptions { get; }
    bool IsCastFromItem { get; }
}

public class CastFromItemOptions
{
    public required IItem Item { get; set; }
    public IEntity? PredefinedTarget { get; set; }
}
