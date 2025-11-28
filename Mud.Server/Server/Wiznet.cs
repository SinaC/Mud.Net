using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;

namespace Mud.Server.Server;

[Export(typeof(IWiznet)), Shared]
public class Wiznet : IWiznet
{
    private ILogger<Wiznet> Logger { get; }
    private IAdminManager AdminManager { get; }

    public Wiznet(ILogger<Wiznet> logger, IAdminManager adminManager)
    {
        Logger = logger;
        AdminManager = adminManager;
    }

    public void Log(string message, WiznetFlags flags, AdminLevels minLevel = AdminLevels.Angel)
    {
        if (flags.HasFlag(WiznetFlags.Bugs))
        {
            Logger.LogError("WIZNET: FLAGS: {flags} {message}", flags, message);
        }
        else if (flags.HasFlag(WiznetFlags.Typos))
        {
            Logger.LogWarning("WIZNET: FLAGS: {flags} {message}", flags, message);
        }
        else
        {
            Logger.LogInformation("WIZNET: FLAGS: {flags} {message}", flags, message);
        }
        foreach (var admin in AdminManager.Admins.Where(a => a.WiznetFlags.HasFlag(flags) && a.Level >= minLevel))
            admin.Send($"%W%WIZNET%x%:{message}");
    }
}
