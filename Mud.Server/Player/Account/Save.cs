using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Player.Account
{
    [PlayerCommand("save", "Misc", Priority = 999 /*low priority*/, NoShortcut = true)]
    public class Save : AccountGameActionBase
    {
        public override void Execute(IActionInput actionInput)
        {
            bool saved = Actor.Save();
            if (saved)
                Actor.Send("Saving. Remember that ROM has automatic saving now.");
            else
                Actor.Send("%r%Save failed%x%");
        }
    }
}
