﻿using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [Command("atalk", Category = "Communication")]
        [Command("admintalk", Category = "Communication")]
        [Syntax("[cmd] <message>")]
        protected virtual bool DoAdminTalk(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("What do you want to say on admin channel ?");
                return true;
            }

            string what = $"%c%[%y%{DisplayName}%c%]: {parameters[0].Value}%x%";
            foreach (IAdmin admin in AdminManager.Admins)
                admin.Send(what);

            return true;
        }
    }
}
