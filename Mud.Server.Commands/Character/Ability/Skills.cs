using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Commands.Character.Ability;

[CharacterCommand("skills", "Ability", Priority = 60)]
[Syntax(
    "[cmd]",
    "[cmd] all")]
public class Skills : AbilitiesCharacterGameActionBase
{
    protected override Func<AbilityTypes, bool> AbilityTypeFilterFunc => x => x == AbilityTypes.Skill || x == AbilityTypes.Passive || x == AbilityTypes.Weapon;
    protected override string Title => "Skills/Passives/Weapons";
}
