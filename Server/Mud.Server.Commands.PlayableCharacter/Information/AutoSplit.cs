using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.PlayableCharacter.Information;

[PlayableCharacterCommand("autosplit", "Information")]
[Syntax("[cmd]")]
[Help(
@"Split up spoils from combat among your group members")]
public class AutoSplit : AutoBase
{
    protected override string What => "Gold";
    protected override string RemovedMessage => "Autosplitting removed.";
    protected override string AddedMessage => "Automatic gold splitting set.";
}
