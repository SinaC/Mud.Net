using Mud.Common;
using Mud.Server.Character.Communication;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Rom24.Races.NonPlayableRaces;
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
            "[cmd] <category>",
            "[cmd] <prefix>")]
    public class Commands : ActorGameAction
    {
        private const int columnCount = 6;

        public bool ShouldDisplayCategories { get; protected set; }
        public ICommandParameter Parameter { get; protected set; }

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
                Parameter = actionInput.Parameters[0];
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

            Func<string, bool> nameFilter;
            Func<string, bool> categoryFilter;

            if (Parameter.IsAll)
            {
                nameFilter = x => true;
                categoryFilter = x => true;
            }
            else
            {
                // if parameter match a category, display category
                string[] categories = filteredCommands.SelectMany(x => x.Value.Categories).ToArray();
                string matchingCategory = categories.FirstOrDefault(x => StringCompareHelpers.StringEquals(x, Parameter.Value));
                if (matchingCategory != null)
                {
                    nameFilter = x => true;
                    categoryFilter = x => StringCompareHelpers.StringEquals(x, matchingCategory);
                }
                // else, filter on name
                else
                {
                    nameFilter = x => StringCompareHelpers.StringStartsWith(x, Parameter.Value);
                    categoryFilter = x => true;
                }
            }

            // Grouped by category
            // if a command has multiple categories, it will appear in each category
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available commands:");
            foreach (var cmdByCategory in filteredCommands
                .SelectMany(x => GetNames(x.Value).Where(n => nameFilter(n)), (kv, name) => new { name, kv.Value })
                .GroupBy(x => x.name, (name, group) => new { name, group.First().Value })
                .SelectMany(x => x.Value.Categories.Where(categoryFilter), (kv, category) => new { category, name = kv.name, priority = kv.Value.Priority })
                .GroupBy(x => x.category, (category, group) => new { category, commands = group })
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

        private IEnumerable<string> GetNames(IGameActionInfo gameActionInfo)
        {
            yield return gameActionInfo.Name;
            foreach (string alias in gameActionInfo.Aliases)
                yield return alias;
        }
    }
}
