using Mud.Server.Interfaces;
using System.Reflection;

namespace Mud.Server
{
    public class AssemblyHelper : IAssemblyHelper
    {
        public Assembly ExecutingAssembly => Assembly.GetExecutingAssembly();
    }
}
