using System;
using System.Collections.Generic;

namespace Mud.Server.Interfaces.GameAction
{
    public interface IGameActionInfo
    {
        string Name { get; }
        int Priority { get; }
        bool Hidden { get; }
        bool NoShortcut { get; }
        bool AddCommandInParameters { get; }
        string[] Categories { get; }

        string[] Syntax { get; }

        string[] Aliases { get; }

        Type CommandExecutionType { get; }

        IEnumerable<string> Names { get; }
    }
}
