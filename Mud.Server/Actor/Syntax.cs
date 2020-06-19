using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.Server.Actor
{
    [Command("syntax", Priority = 999)]
    [Syntax(
            "[cmd] all",
            "[cmd] <command>")]
    public class Syntax : ActorGameAction
    {
        public string CommandName { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return BuildCommandSyntax();

            CommandName = actionInput.Parameters[0].IsAll
                ? string.Empty // Trie will return whole tree when searching with empty string
                : actionInput.Parameters[0].Value.ToLowerInvariant();
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            var commands = Actor.Commands.GetByPrefix(CommandName).Where(x => !x.Value.Hidden);

            bool found = false;
            StringBuilder sb = new StringBuilder();
            foreach (var gameActionInfo in commands.Select(x => x.Value).DistinctBy(x => x.Name).OrderBy(x => x.Name))
            {
                string[] names = GetNames(gameActionInfo).ToArray();
                string title = string.Join(", ", names.Select(x => $"%C%{x}%x%"));
                sb.AppendLine($"Command{(names.Length > 1 ? "s" : string.Empty)} {title}:");
                string commandNames = string.Join("|", names);
                StringBuilder sbSyntax = BuildCommandSyntax(commandNames, gameActionInfo.Syntax, true);
                sb.Append(sbSyntax);
                found = true;
            }
            if (found)
                Actor.Page(sb);
            else
                Actor.Send("No command found.");
        }

        private IEnumerable<string> GetNames(IGameActionInfo gameActionInfo)
        {
            yield return gameActionInfo.Name;
            foreach (string alias in gameActionInfo.Aliases)
                yield return alias;
        }
    }
}
