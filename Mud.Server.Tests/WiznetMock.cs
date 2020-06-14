using System.Diagnostics;
using Mud.Domain;

namespace Mud.Server.Tests
{
    internal class WiznetMock : IWiznet
    {
        public void Wiznet(string message, WiznetFlags flags, AdminLevels minLevel = AdminLevels.Angel)
        {
            Debug.WriteLine("WIZNET:" + message);
        }
    }
}
