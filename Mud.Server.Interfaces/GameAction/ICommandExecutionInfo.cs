using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Interfaces.GameAction
{
    public interface ICommandExecutionInfo
    {
        string Name { get; }
        int Priority { get; }
        bool Hidden { get; }
        bool NoShortcut { get; }
        bool AddCommandInParameters { get; }
        string[] Categories { get; }

        string[] Syntax { get; }
    }
}
