using Mud.Domain;
using Mud.Logger;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;

namespace Mud.Server.Server
{
    public class Wiznet : IWiznet
    {
        private IAdminManager AdminManager { get; }

        public Wiznet(IAdminManager adminManager)
        {
            AdminManager = adminManager;
        }

        public void Log(string message, WiznetFlags flags, AdminLevels minLevel = AdminLevels.Angel)
        {
            LogLevels level = LogLevels.Info;
            if (flags.HasFlag(WiznetFlags.Bugs))
                level = LogLevels.Error;
            else if (flags.HasFlag(WiznetFlags.Typos))
                level = LogLevels.Warning;
            Logger.Log.Default.WriteLine(level, "WIZNET: FLAGS: {0} {1}", flags, message);
            foreach (var admin in AdminManager.Admins.Where(a => a.WiznetFlags.HasFlag(flags) && a.Level >= minLevel))
                admin.Send($"%W%WIZNET%x%:{message}");
        }
    }
}
