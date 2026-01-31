using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Character.PlayableCharacter.Group;

[PlayableCharacterCommand("leave", "Group")]
[Syntax("[cmd] <member>")]
[Help(@"[cmd] makes you leave a group.")]
public class Leave : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [];

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
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
