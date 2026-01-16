using Mud.Server.Domain;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.Ability;

[CharacterCommand("abilities", "Ability")]
[Syntax(
    "[cmd]",
    "[cmd] all")]
public class Abilities : AbilitiesCharacterGameActionBase
{
    protected override Func<AbilityTypes, bool> AbilityTypeFilterFunc => x => true;
    protected override string Title => "Abilities";
}
