using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Information
{
    [CharacterCommand("time", "Information")]
    [Syntax("[cmd]")]
    public class Time : CharacterGameAction
    {
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
}
