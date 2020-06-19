using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.Server.Actor
{
    [Command("commands", Priority = 0)]
    [Alias("cmd")]
    [Syntax(
            "[cmd]",
            "[cmd] all",
            "[cmd] <category>")]
    public class Commands : ActorGameAction
    {
        private const int columnCount = 6;

        public bool ShouldDisplayCategories { get; protected set; }
        public Func<string, bool> CategoryFilter { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                ShouldDisplayCategories = true;
            else
            {
                ShouldDisplayCategories = false;
                // If a parameter is specified, filter on category unless parameter is 'all'
                if (actionInput.Parameters[0].IsAll)
                    CategoryFilter = _ => true;
                else
                    CategoryFilter = category => StringCompareHelpers.StringStartsWith(category, actionInput.Parameters[0].Value);
            }
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            // Display categories
            if (ShouldDisplayCategories)
                DisplayCategories();
            else
                DisplayCommands();
        }

        private void DisplayCategories()
        {
            IEnumerable<KeyValuePair<string, IGameActionInfo>> filteredCommands = Actor.Commands.Where(x => !x.Value.Hidden);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available categories:%W%");
            int index = 0;
            foreach (var category in filteredCommands
                .SelectMany(x => x.Value.Categories.Where(c => !string.IsNullOrWhiteSpace(c)))
                .Distinct()
                .OrderBy(x => x))
            {
                sb.AppendFormat("{0,-14}", category);
                if ((++index % columnCount) == 0)
                    sb.AppendLine();
            }
            if (index > 0 && index % columnCount != 0)
                sb.AppendLine();
            sb.Append("%x%");
            Actor.Page(sb);
        }

        private void DisplayCommands()
        {
            IEnumerable<KeyValuePair<string, IGameActionInfo>> filteredCommands = Actor.Commands.Where(x => !x.Value.Hidden);

            // Grouped by category
            // if a command has multiple categories, it will appear in each category
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available commands:");
            foreach (var cmdByCategory in filteredCommands
                .SelectMany(x => x.Value.Categories.Where(CategoryFilter), (kv, category) => new { category, name = kv.Key, priority = kv.Value.Priority })
                .GroupBy(x => x.category, (category, group) => new { category, commands = group})
                .OrderBy(g => g.category))
            {
                if (!string.IsNullOrEmpty(cmdByCategory.category))
                    sb.AppendLine("%W%" + cmdByCategory.category + ":%x%");
                int index = 0;
                foreach (var cmdInfo in cmdByCategory.commands
                    .OrderBy(x => x.priority)
                    .ThenBy(x => x.name))
                {
                    sb.AppendFormat("{0,-14}", cmdInfo.name);
                    if ((++index % columnCount) == 0)
                        sb.AppendLine();
                }
                if (index > 0 && index % columnCount != 0)
                    sb.AppendLine();
            }
            Actor.Page(sb);
        }
    }
}
