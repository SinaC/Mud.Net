using Mud.Server.Constants;
using Mud.Server.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("delete", Category = "Misc", Priority = 999, NoShortcut = true)]
        protected override bool DoDelete(string rawParameters, params CommandParameter[] parameters)
        {
            Send("An admin cannot be deleted from game!!!");
            return true;
        }
    }
}
