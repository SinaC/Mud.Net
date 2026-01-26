using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

[PlayableCharacterCommand("autogold", "Information")]
[Syntax("[cmd]")]
[Help(
@"Take all gold from dead mobiles")]
public class AutoGold : AutoBase
{
    protected override string What => "Gold";
    protected override string RemovedMessage => "Autogold removed.";
    protected override string AddedMessage => "Automatic gold looting set.";
}
