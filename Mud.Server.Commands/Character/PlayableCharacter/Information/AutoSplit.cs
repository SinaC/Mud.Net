using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

[PlayableCharacterCommand("autosplit", "Information")]
[Syntax("[cmd]")]
[Help(
@"Split up spoils from combat among your group members")]
public class AutoSplit : AutoBase
{
    protected override AutoFlags What => AutoFlags.Gold;
    protected override string RemovedMessage => "Autosplitting removed.";
    protected override string AddedMessage => "Automatic gold splitting set.";
}
