using Mud.Common;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("visible", "Movement"), MinPosition(Positions.Sleeping)]
[Syntax("[cmd]")]
[Help(
@"[cmd] cancels your hiding and sneaking, as well as any invisibility,
making you visible again.")]
public class Visible : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.RemoveBaseCharacterFlags(true, "Invisible", "Sneak", "Hide");
        Actor.RemoveAuras(x => StringCompareHelpers.StringEquals(x.AbilityName, "Invisibility") // TODO find another way
                         || StringCompareHelpers.StringEquals(x.AbilityName, "Sneak")
                         || StringCompareHelpers.StringEquals(x.AbilityName, "Hide"), true, true);
        Actor.Send("You are now visible");
    }
}
