using Mud.Domain;

namespace Mud.Server
{
    public interface IWiznet
    {
        void Wiznet(string message, WiznetFlags flags, AdminLevels minLevel = AdminLevels.Angel);

    }
}
