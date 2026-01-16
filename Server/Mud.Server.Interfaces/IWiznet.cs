using Mud.Domain;

namespace Mud.Server.Interfaces;

public interface IWiznet
{
    void Log(string message, WiznetFlags flags, AdminLevels minLevel = AdminLevels.Angel);

}
