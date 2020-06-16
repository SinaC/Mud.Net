using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("impersonate", "Avatar", Priority = 0)]
        [Syntax(
            "[cmd]",
            "[cmd] <character>")]
        protected override CommandExecutionResults DoImpersonate(string rawParameters, params ICommandParameter[] parameters)
        {
            if (Incarnating != null)
                Send("You are already incarnating {0}.", Incarnating.DisplayName);
            else
                return base.DoImpersonate(rawParameters, parameters);
            return CommandExecutionResults.Ok;
        }
    }
}
