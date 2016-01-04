using System;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("impersonate")]
        protected override bool DoImpersonate(string rawParameters, params CommandParameter[] parameters)
        {
            if (Incarnating != null)
                Send("You are already incarnating {0}." + Environment.NewLine, Incarnating.DisplayName);
            else
                return base.DoImpersonate(rawParameters, parameters);
            return true;
        }
    }
}
