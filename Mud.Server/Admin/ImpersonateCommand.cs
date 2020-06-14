using Mud.Server.Input;
// ReSharper disable UnusedMember.Global

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

        [AdminCommand("immortal", "Avatar", Priority = 500, MustBeImpersonated = true)]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoImmortal(string rawParameters, params CommandParameter[] parameters)
        {
            if (Impersonating.IsImmortal)
            {
                Send("%R%BEWARE: %G%YOU ARE %R%MORTAL%G% NOW!%x%");
                Impersonating.ChangeImmortalState(false);
            }
            else
            {
                Send("%R%YOU ARE IMMORTAL NOW!%x%");
                Impersonating.ChangeImmortalState(true);
            }

            return CommandExecutionResults.Ok;
        }
    }
}
