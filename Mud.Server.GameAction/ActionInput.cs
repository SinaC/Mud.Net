using Mud.Server.Input;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction
{
    public class ActionInput : IActionInput
    {
        public IActor Actor { get; }
        public string CommandLine { get; }
        public string Command { get; }
        public string RawParameters { get; }
        public CommandParameter[] Parameters { get; }
        public IGameActionInfo CommandInfo { get; }

        public object Context { get; set; }

        public ActionInput(IGameActionInfo commandInfo, IActor actor, string commandLine)
        {
            CommandInfo = commandInfo;
            Actor = actor;
            CommandLine = commandLine;
            CommandHelpers.ExtractCommandAndParameters(commandLine, out var command, out var rawParameters, out var parameters); // TODO: move code to this class
            Command = command;
            RawParameters = rawParameters;
            Parameters = parameters;
        }

        public ActionInput(IGameActionInfo commandInfo, IActor actor, string commandLine, string command, string rawParameters, params CommandParameter[] parameters)
        {
            CommandInfo = commandInfo;
            Actor = actor;
            CommandLine = commandLine;
            Command = command;
            RawParameters = rawParameters;
            Parameters = parameters;
        }
    }
}
