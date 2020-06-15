using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using System.Collections.Generic;
using System.Text;

namespace Mud.Server.GameAction
{
    public abstract class GameActionBase<TActor> : IGameAction
        where TActor: class, IActor
    {
        protected IGameActionInfo CommandInfo { get; private set; }

        public TActor Actor { get; protected set; }

        public abstract void Execute(IActionInput actionInput);

        public virtual string Guards(IActionInput actionInput)
        {
            CommandInfo = actionInput.CommandInfo;
            if (CommandInfo == null)
                return "Internal error: CommandInfo is null.";
            Actor = actionInput.Actor as TActor;
            if (Actor == null)
                return $"This command must be executed by {typeof(TActor).Name}";
            return null;
        }

        public static StringBuilder BuildCommandSyntax(string commandNames, IEnumerable<string> syntaxes, bool addSpaces)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string syntax in syntaxes)
            {
                // TODO: enrich argument such as <character>, <player name>, ...
                string enrichedSyntax = syntax.Replace("[cmd]", commandNames);
                if (addSpaces)
                    sb.Append("      ");
                sb.AppendLine("Syntax: " + enrichedSyntax);
            }
            return sb;
        }

        protected string BuildCommandSyntax()
        {
            return BuildCommandSyntax(CommandInfo.Name, CommandInfo.Syntax, false).ToString();
        }
    }
}
