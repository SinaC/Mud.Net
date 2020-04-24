using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("impersonate", "Avatar", Priority = 0)]
        [Syntax(
            "[cmd]",
            "[cmd] <character>")]
        protected override CommandExecutionResults DoImpersonate(string rawParameters, params CommandParameter[] parameters)
        {
            if (Incarnating != null)
                Send("You are already incarnating {0}.", Incarnating.DisplayName);
            else
                return base.DoImpersonate(rawParameters, parameters);
            return CommandExecutionResults.Ok;
        }
    }
}
