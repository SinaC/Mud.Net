using System.Collections.Generic;

namespace Mud.Server
{
    public interface ICommandProcessor
    {
        bool ProcessCommand(IClient client, string commandLine);
        List<string> CommandList(CommandFlags flags);
    }
}
