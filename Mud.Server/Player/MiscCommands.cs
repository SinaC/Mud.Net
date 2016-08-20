using System;
using Mud.Server.Input;
using Mud.Server.Server;

namespace Mud.Server.Player
{
    // TODO: find a better filename
    public partial class Player
    {
        //[Command("qui", Hidden = true)] // TODO: full match
        //protected virtual bool DoQui(string rawParameters, params CommandParameter[] parameters)
        //{
        //    Send("If you want to QUIT, spell it out." + Environment.NewLine);
        //    return true;
        //}

        [Command("quit", Priority = 999/*low priority*/, NoShortcut = true)]
        protected virtual bool DoQuit(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: in combat check, ...
            Repository.Server.Quit(this);
            return true;
        }

        [Command("password", Priority = 999, NoShortcut = true)]
        protected virtual bool DoPassword(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length != 2)
            {
                Send("Syntax: password <old> <new>"+Environment.NewLine);
                return true;
            }
            if (!Repository.LoginManager.CheckPassword(Name, parameters[0].Value))
            {
                Send("Wrong password. Wait 10 seconds."+Environment.NewLine);
                SetGlobalCooldown(10*ServerOptions.PulsePerSeconds);
                return true;
            }
            if (parameters[1].Value.Length < 5)
            {
                Send("New password must be at least five characters long."+Environment.NewLine);
                return true;
            }
            Repository.LoginManager.ChangePassword(Name, parameters[1].Value);
            return true;
        }
    }
}
