using Mud.Server.Domain;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.Ability;

[CharacterCommand("Weapons", "Ability", Priority = 60)]
[Syntax(
    "[cmd]",
    "[cmd] all")]
public class Weapons : AbilitiesCharacterGameActionBase
{
    protected override Func<AbilityTypes, bool> AbilityTypeFilterFunc => x => x == AbilityTypes.Weapon;
    protected override string Title => "Weapons";
}
