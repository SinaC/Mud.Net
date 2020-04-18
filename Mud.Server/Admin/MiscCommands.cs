using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("delete", Category = "Misc", Priority = 999, NoShortcut = true)]
        protected override bool DoDelete(string rawParameters, params CommandParameter[] parameters)
        {
            Send("An admin cannot be deleted in game!!!");
            return true;
        }
    }
}
