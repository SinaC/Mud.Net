using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Interfaces.GameAction;

namespace Mud.POC.Abilities2
{
    public interface IActionInput
    {
        IActor Actor { get; }
        string CommandLine { get; }
        string Command { get; }
        string RawParameters { get; }
        ICommandParameter[] Parameters { get; }

        object Context { get; set; }
    }
}
