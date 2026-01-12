using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

[PlayableCharacterCommand("autosacrifice", "Information")]
[Syntax("[cmd]")]
[Help(
@"Sacrifice dead monsters (if autoloot is on, only empty corpes)")]
public class AutoSacrifice : AutoBase
{
    protected override AutoFlags What => AutoFlags.Sacrifice;
    protected override string RemovedMessage => "Autosacrificing removed.";
    protected override string AddedMessage => "Automatic corpse sacrificing set.";
}
