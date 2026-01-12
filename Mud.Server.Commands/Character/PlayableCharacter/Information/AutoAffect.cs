using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

[PlayableCharacterCommand("autoaffect", "Information")]
[Syntax("[cmd]")]
[Help(
@"Display your affects when looking your score")]
public class AutoAffect : AutoBase
{
    protected override AutoFlags What => AutoFlags.Affect;
    protected override string RemovedMessage => "Autoaffect removed.";
    protected override string AddedMessage => "Affects will now be displayed when looking your score.";
}
