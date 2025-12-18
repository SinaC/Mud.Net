using Mud.Server.Domain;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.Ability;

[CharacterCommand("spells", "Ability")]
[Syntax(
    "[cmd]",
    "[cmd] all")]
public class Spells : AbilitiesCharacterGameActionBase
{
    protected override Func<AbilityTypes, bool> AbilityTypeFilterFunc => x => x == AbilityTypes.Spell;
    protected override string Title => "Spells";
}
