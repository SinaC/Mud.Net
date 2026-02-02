using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

[PlayableCharacterCommand("autoaffect", "Information")]
[Syntax("[cmd]")]
[Help(
@"Display your affects when looking your score")]
public class AutoAffect : AutoBase
{
    protected override string What => "Affect";
    protected override string RemovedMessage => "Autoaffect removed.";
    protected override string AddedMessage => "Affects will now be displayed when looking your score.";
}
