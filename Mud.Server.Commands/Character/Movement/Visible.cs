using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("visible", "Movement", MinPosition = Positions.Sleeping)]
[Syntax("[cmd]")]
[Help(
@"[cmd] cancels your hiding and sneaking, as well as any invisibility,
making you visible again.")]
public class Visible : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.RemoveBaseCharacterFlags(true, "Invisible", "Sneak", "Hide");
        Actor.RemoveAuras(x => x.AbilityName == "Invisibility"
                         || x.AbilityName == "Sneak"
                         || x.AbilityName == "Hide", true);
        Actor.Send("You are now visible");
    }
}
