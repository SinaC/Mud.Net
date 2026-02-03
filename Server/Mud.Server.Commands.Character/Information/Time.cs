using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("time", "Information")]
[Syntax("[cmd]")]
[Help(
@"[cmd] shows the game time, as well as the time the mud was last started,
and the current local time for the host computer.")]
public class Time : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [];

    private ITimeManager TimeManager { get; }

    public Time(ITimeManager timeManager)
    {
        TimeManager = timeManager;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Send(TimeManager.TimeInfo());
    }
}
