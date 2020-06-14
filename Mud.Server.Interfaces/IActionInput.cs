using Mud.Server.Input;
using Mud.Server.Interfaces.Actor;

namespace Mud.Server.Interfaces
{
    public interface IActionInput
    {
        IActor Actor { get; }
        string CommandLine { get; }
        string Command { get; }
        string RawParameters { get; }
        CommandParameter[] Parameters { get; }

        object Context { get; set; }
    }
}
