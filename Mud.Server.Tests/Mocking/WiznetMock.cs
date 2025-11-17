using System.Diagnostics;
using Mud.Domain;
using Mud.Server.Interfaces;

namespace Mud.Server.Tests.Mocking
{
    internal class WiznetMock : IWiznet
    {
        public void Log(string message, WiznetFlags flags, AdminLevels minLevel = AdminLevels.Angel)
        {
            Debug.WriteLine("WIZNET:" + message);
        }
    }
}
