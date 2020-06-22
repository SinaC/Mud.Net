using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.GameAction;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("dumpcommands")]
    public class DumpCommands : AdminGameAction
    {
        private IGameActionManager GameActionManager { get; }

        public DumpCommands(IGameActionManager gameActionManager)
        {
            GameActionManager = gameActionManager;
        }

        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = TableGenerators.GameActionInfoTableGenerator.Value.Generate("Commands", GameActionManager.GameActions.OrderBy(x => x.Name));
            Actor.Page(sb);
        }
    }
}
