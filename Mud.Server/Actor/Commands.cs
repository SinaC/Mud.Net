using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Input;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.Server.Actor
{
    [Command("cmd", Priority = 0)]
    [Command("commands", Priority = 0)]
    [Syntax(
            "[cmd]",
            "[cmd] all",
            "[cmd] <category>")]
    public class Commands : GameActionBase<IActor>
    {
        private const int columnCount = 6;

        public bool ShouldDisplayCategories { get; protected set; }
        public Func<string, bool> CategoryFilter { get; protected set; }

        public override void Execute(IActionInput actionInput)
        {
            // Display categories
            if (ShouldDisplayCategories)
                DisplayCategories();
            else
                DisplayCommands();
        }

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

        private void DisplayCategories()
        {
            IEnumerable<KeyValuePair<string, CommandExecutionInfo>> filteredCommands = Actor.Commands.Where(x => !x.Value.CommandAttribute.Hidden && Actor.IsCommandAvailable(x.Value.CommandAttribute));

            StringBuilder categoriesSb = new StringBuilder();
            categoriesSb.AppendLine("Available categories:%W%");
            int index = 0;
            foreach (var category in filteredCommands
                .SelectMany(x => x.Value.CommandAttribute.Categories.Where(c => !string.IsNullOrWhiteSpace(c)))
                .Distinct()
                .OrderBy(x => x))
            {
                if ((++index % columnCount) == 0)
                    categoriesSb.AppendFormatLine("{0,-14}", category);
                else
                    categoriesSb.AppendFormat("{0,-14}", category);
            }
            if (index > 0 && index % columnCount != 0)
                categoriesSb.AppendLine();
            categoriesSb.Append("%x%");
            Actor.Send(categoriesSb);
        }

        private void DisplayCommands()
        {
            IEnumerable<KeyValuePair<string, CommandExecutionInfo>> filteredCommands = Actor.Commands.Where(x => !x.Value.CommandAttribute.Hidden && Actor.IsCommandAvailable(x.Value.CommandAttribute));

            // Grouped by category
            // if a command has multiple categories, it will appear in each category
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available commands:");
            foreach (var cmdByCategory in filteredCommands
                .SelectMany(x => x.Value.CommandAttribute.Categories.Where(CategoryFilter), (kv, category) => new { category, cmi = kv.Value })
                .GroupBy(x => x.category, (category, group) => new { category, commands = group.Select(x => x.cmi) })
                .OrderBy(g => g.category))
            {
                if (!string.IsNullOrEmpty(cmdByCategory.category))
                    sb.AppendLine("%W%" + cmdByCategory.category + ":%x%");
                int index = 0;
                foreach (CommandExecutionInfo ci in cmdByCategory.commands.OfType<CommandExecutionInfo>()
                    .OrderBy(x => x.CommandAttribute.Priority)
                    .ThenBy(x => x.CommandAttribute.Name))
                {
                    if ((++index % columnCount) == 0)
                        sb.AppendFormatLine("{0,-14}", ci.CommandAttribute.Name);
                    else
                        sb.AppendFormat("{0,-14}", ci.CommandAttribute.Name);
                }
                if (index > 0 && index % columnCount != 0)
                    sb.AppendLine();
            }
            Actor.Page(sb);
        }
    }
}
