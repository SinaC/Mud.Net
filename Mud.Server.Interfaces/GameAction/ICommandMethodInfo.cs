using System.Reflection;

namespace Mud.Server.Interfaces.GameAction
{
    public interface ICommandMethodInfo : ICommandExecutionInfo
    {

        MethodInfo MethodInfo { get; }
    }
}
