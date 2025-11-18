using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("weather", "Information", MinPosition = Positions.Resting)]
[Syntax("[cmd]")]
public class Weather : CharacterGameAction
{
    private ITimeManager TimeManager { get; }

    public Weather(ITimeManager timeManager)
    {
        TimeManager = timeManager;
    }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Room == null)
            return "You are nowhere!";

        if (Actor.Room.RoomFlags.IsSet("Indoors"))
            return "You can't see the weather indoors.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        string change = TimeManager.PressureChange >= 0
            ? "a warm southerly breeze blows"
            : "a cold northern gust blows";
        Actor.Send("The sky is {0} and {1}.", TimeManager.SkyState.PrettyPrint(), change);

        if (TimeManager.IsMoonNight())
        {
            for (int i = 0; i < TimeManager.MoonCount; i++)
                if (TimeManager.IsMoonVisible(i))
                    Actor.Send(TimeManager.MoonInfo(i));
        }
    }
}
