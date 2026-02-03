using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.PlayableCharacter.Information;

[PlayableCharacterCommand("autosacrifice", "Information")]
[Syntax("[cmd]")]
[Help(
@"Sacrifice dead monsters (if autoloot is on, only empty corpes)")]
public class AutoSacrifice : AutoBase
{
    protected override string What => "Sacrifice";
    protected override string RemovedMessage => "Autosacrificing removed.";
    protected override string AddedMessage => "Automatic corpse sacrificing set.";
}
