using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Player.Communication
{
    [PlayerCommand("afk", "Communication")]
    public class Afk : PlayerGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            Actor.ToggleAfk();
        }
    }
}
