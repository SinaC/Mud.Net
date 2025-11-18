using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Group;

[PlayableCharacterCommand("leave", "Group")]
[Syntax("[cmd] <member>")]
[Help(@"[cmd] makes you leave a group.")]
public class Leave : PlayableCharacterGameAction
{
    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Group == null)
            return "You aren't in a group.";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Actor.Group!.Members.Count() <= 2) // group will contain only one member, disband
            Actor.Group.Disband();
        else
            Actor.Group.RemoveMember(Actor);

    }
}
