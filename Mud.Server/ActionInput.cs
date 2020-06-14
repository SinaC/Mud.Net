using Mud.Server.Input;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Actor;

namespace Mud.Server
{
    // TODO: move in a specific project with command parsing
    public class ActionInput : IActionInput
    {
        public IActor Actor { get; }
        public string CommandLine { get; }
        public string Command { get; }
        public string RawParameters { get; }
        public CommandParameter[] Parameters { get; }

        public object Context { get; set; }

        public ActionInput(IActor actor, string commmandLine)
        {
            Actor = actor;
            CommandLine = commmandLine;
            CommandHelpers.ExtractCommandAndParameters(commmandLine, out var command, out var rawParameters, out var parameters); // TODO: move code to this class
            Command = command;
            RawParameters = rawParameters;
            Parameters = parameters;
        }

        public ActionInput(IActor actor, string commandLine, string command, string rawParameters, params CommandParameter[] parameters)
        {
            Actor = actor;
            CommandLine = commandLine;
            Command = command;
            RawParameters = rawParameters;
            Parameters = parameters;
        }
    }
}
