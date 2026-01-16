using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

[PlayableCharacterCommand("autoall", "Information")]
[Syntax("[cmd]")]
[Help(
@"Active every 'auto'")]
public class AutoAll : PlayableCharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        foreach (var autoFlag in Enum.GetValues<AutoFlags>().Where(x => x != AutoFlags.None).OrderBy(x => x.ToString()))
            Actor.AddAutoFlags(autoFlag);
        Actor.Send("Ok.");
    }
}
