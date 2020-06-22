using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using System.Collections.Generic;
using System.Text;

namespace Mud.Server.GameAction
{
    public abstract class GameActionBase<TActor, TGameActionInfo> : IGameAction
        where TActor: class, IActor
        where TGameActionInfo: class, IGameActionInfo
    {
        protected TGameActionInfo GameActionInfo { get; private set; }

        public TActor Actor { get; private set; }

        public abstract void Execute(IActionInput actionInput);

        public virtual string Guards(IActionInput actionInput)
        {
            GameActionInfo = actionInput.GameActionInfo as TGameActionInfo;
            if (GameActionInfo == null)
                return $"Internal error: GameActionInfo is null or not of type {typeof(TGameActionInfo).Name}. GameActionInfo type is {actionInput.GameActionInfo.GetType().Name}.";
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
            return BuildCommandSyntax(GameActionInfo.Name, GameActionInfo.Syntax, false).ToString();
        }
    }
}
