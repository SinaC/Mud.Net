using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
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

        public override void Execute(IActionInput actionInput)
        {
            var commands = Actor.Commands.GetByPrefix(CommandName).Where(x => !x.Value.Hidden);

            bool found = false;
            StringBuilder sb = new StringBuilder();
            foreach (var group in commands.Select(x => x.Value).GroupBy(x => x.Name).OrderBy(x => x.Key)) // group by command
            {
                string[] namesByPriority = group.OrderBy(x => x.Priority).Select(x => x.Name).ToArray(); // order by priority
                string title = string.Join(", ", namesByPriority.Select(x => $"%C%{x}%x%"));
                sb.AppendLine($"Command{(namesByPriority.Length > 1 ? "s" : string.Empty)} {title}:");
                string commandNames = string.Join("|", namesByPriority);
                StringBuilder sbSyntax = BuildCommandSyntax(commandNames, group.SelectMany(x => x.Syntax).Distinct(), true);
                sb.Append(sbSyntax);
                found = true;
            }
            if (found)
                Actor.Page(sb);
            else
                Actor.Send("No command found.");
        }

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
    }
}
