using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IWiznet
    {
        void Wiznet(string message, WiznetFlags flags, AdminLevels minLevel = AdminLevels.Angel);

    }
}
