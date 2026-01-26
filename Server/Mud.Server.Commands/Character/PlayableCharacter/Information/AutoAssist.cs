using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

[PlayableCharacterCommand("autoassist", "Information")]
[Syntax("[cmd]")]
[Help(
@"Makes you help group members in combat")]
public class AutoAssist : AutoBase
{
    protected override string What => "Assist";
    protected override string RemovedMessage => "Autoassist removed.";
    protected override string AddedMessage => "You will now assist when needed.";
}
