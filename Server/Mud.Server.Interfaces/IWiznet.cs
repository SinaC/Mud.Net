using Mud.Domain;
using Mud.Flags.Interfaces;

namespace Mud.Server.Interfaces;

public interface IWiznet
{
    void Log(string message, IWiznetFlags flags, AdminLevels minLevel = AdminLevels.Angel);

}
