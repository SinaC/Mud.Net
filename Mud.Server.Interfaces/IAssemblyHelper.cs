using System.Reflection;

namespace Mud.Server.Interfaces
{
    public interface IAssemblyHelper
    {
        Assembly ExecutingAssembly { get; }
    }
}
