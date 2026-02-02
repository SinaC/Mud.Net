using Mud.Common;
using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("visible", "Movement")]
[Syntax("[cmd]")]
[Help(
@"[cmd] cancels your hiding and sneaking, as well as any invisibility,
making you visible again.")]
public class Visible : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Sleeping)];

    public override void Execute(IActionInput actionInput)
    {
        Actor.RemoveBaseCharacterFlags(true, "Invisible", "Sneak", "Hide");
        Actor.RemoveAuras(x => StringCompareHelpers.StringEquals(x.AbilityName, "Invisibility") // TODO find another way
                         || StringCompareHelpers.StringEquals(x.AbilityName, "Sneak")
                         || StringCompareHelpers.StringEquals(x.AbilityName, "Hide"), true, true);
        Actor.Send("You are now visible");
    }
}
