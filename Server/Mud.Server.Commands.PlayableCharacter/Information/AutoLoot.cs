using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.PlayableCharacter.Information;

[PlayableCharacterCommand("autoloot", "Information")]
[Syntax("[cmd]")]
[Help(
@"Take all equipment from dead mobiles")]
public class AutoLoot : AutoBase
{
    protected override string What => "Loot";
    protected override string RemovedMessage => "Autolooting removed.";
    protected override string AddedMessage => "Automatic corpse looting set.";
}
