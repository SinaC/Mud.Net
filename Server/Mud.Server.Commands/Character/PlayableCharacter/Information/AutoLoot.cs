using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

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
