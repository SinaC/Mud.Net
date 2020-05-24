using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.Interfaces
{
    public interface IWiznet
    {
        void Wiznet(string message, WiznetFlags flags, AdminLevels minLevel = AdminLevels.Angel);

    }
}
