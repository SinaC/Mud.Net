using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Character.Ability;

[CharacterCommand("abilities", "Ability")]
[Syntax(
    "[cmd]",
    "[cmd] all")]
public class Abilities : AbilitiesCharacterGameActionBase
{
    protected override Func<AbilityTypes, bool> AbilityTypeFilterFunc => x => true;
    protected override string Title => "Abilities";
}
