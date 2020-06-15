using System;

namespace Mud.Server.Interfaces.GameAction
{
    public interface ICommandInfo
    {
        string Name { get; }
        int Priority { get; }
        bool Hidden { get; }
        bool NoShortcut { get; }
        bool AddCommandInParameters { get; }
        string[] Categories { get; }

        string[] Syntax { get; }

        Type CommandExecutionType { get; }
    }
}
