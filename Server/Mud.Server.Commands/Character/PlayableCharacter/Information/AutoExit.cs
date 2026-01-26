using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

[PlayableCharacterCommand("autoexit", "Information")]
[Syntax("[cmd]")]
[Help(
@"Display room exits upon entering a room")]
public class AutoExit : AutoBase
{
    protected override string What => "Exit";
    protected override string RemovedMessage => "Exits will no longer be displayed.";
    protected override string AddedMessage => "Exits will now be displayed.";
}
