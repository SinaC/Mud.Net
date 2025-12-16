using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Commands.Character.Ability;

[CharacterCommand("Passives", "Ability", Priority = 60)]
[Syntax(
    "[cmd]",
    "[cmd] all")]
public class Passives : AbilitiesCharacterGameActionBase
{
    protected override Func<AbilityTypes, bool> AbilityTypeFilterFunc => x => x == AbilityTypes.Passive;
    protected override string Title => "Passives";
}
