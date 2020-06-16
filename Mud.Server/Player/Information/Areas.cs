using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.World;
using System.Text;

namespace Mud.Server.Player.Information
{
    [AdminCommand("areas", "Information", Priority = 10)]
    public class Areas : AdminGameAction
    {
        private IWorld World { get; }

        public Areas(IWorld world)
        {
            World = world;
        }

        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb;
            if (Actor is IAdmin)
                sb = TableGenerators.FullInfoAreaTableGenerator.Value.Generate("Areas", World.Areas);
            else
                sb = TableGenerators.AreaTableGenerator.Value.Generate("Areas", World.Areas);
            Actor.Page(sb);
        }
    }
}
