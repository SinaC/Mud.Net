using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Admin.Account
{
    [AdminCommand("delete", "Misc", Priority = 999, NoShortcut = true)]
    public class Delete : AdminGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            Actor.Send("An admin cannot be deleted in game!!!");
        }
    }
}
