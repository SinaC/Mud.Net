using Microsoft.Extensions.Options;
using Mud.Server.Domain.Attributes;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Options;

namespace Mud.Server.Commands.Character.Ability;

[CharacterCommand("Weapons", "Ability")]
[Syntax(
    "[cmd]",
    "[cmd] all",
    "[cmd] <max>",
    "[cmd] <min> <max>")]
[Help(
@"[cmd] without an argument tells you basic information such as available
level, current ability level, resource cost for every weapons you've
already learned and below your current level.
[cmd] all tells you the same information but doesn't take into account your
current level.
[cmd] all same as above without level check.
[cmd] <max> same as above until level <max>.
[cmd] <min> <max> same as above in level range <min> <max>")]
public class Weapons : AbilitiesCharacterGameActionBase<ICharacter, ICharacterGameActionInfo>
{
    protected override Func<AbilityTypes, bool> AbilityTypeFilterFunc => x => x == AbilityTypes.Weapon;
    protected override string Title => "Weapons";
    public Weapons(IOptions<WorldOptions> worldOptions)
        : base(worldOptions)
    {
    }

}
