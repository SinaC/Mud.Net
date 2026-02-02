using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Extensions;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("weather", "Information")]
[Syntax("[cmd]")]
[Help(@"[cmd] shows the current game weather.")]
public class Weather : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting)];

    private ITimeManager TimeManager { get; }

    public Weather(ITimeManager timeManager)
    {
        TimeManager = timeManager;
    }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
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
