using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class ActionInput : IActionInput
{
    public IActor Actor { get; }
    public string CommandLine { get; }
    public string Command { get; }
    public ICommandParameter[] Parameters { get; }
    public IGameActionInfo GameActionInfo { get; }

    public object Context { get; set; } = default!;

    public ActionInput(IGameActionInfo commandInfo, IActor actor, string commandLine, string command, params ICommandParameter[] parameters)
    {
        GameActionInfo = commandInfo;
        Actor = actor;
        CommandLine = commandLine;
        Command = command;
        Parameters = parameters;
    }
}
