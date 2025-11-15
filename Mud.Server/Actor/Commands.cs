using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Actor;

[Command("commands", Priority = 0)]
[Alias("cmd")]
[Syntax(
        "[cmd]",
        "[cmd] all",
        "[cmd] <category>",
        "[cmd] <prefix>")]
public class Commands : ActorGameAction
{
    private const int ColumnCount = 5;

    protected bool ShouldDisplayCategories { get; set; }
    protected ICommandParameter Parameter { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
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
        IEnumerable<KeyValuePair<string, IGameActionInfo>> filteredFameActions = Actor.GameActions.Where(x => !x.Value.Hidden);

        StringBuilder sb = new();
        sb.AppendLine("Available categories:%W%");
        int index = 0;
        foreach (var category in filteredFameActions
            .SelectMany(x => x.Value.Categories.Where(c => !string.IsNullOrWhiteSpace(c)))
            .Distinct()
            .OrderBy(x => x))
        {
            sb.AppendFormat("{0,-14}", category);
            if ((++index % ColumnCount) == 0)
                sb.AppendLine();
        }
        if (index > 0 && index % ColumnCount != 0)
            sb.AppendLine();
        sb.Append("%x%");
        Actor.Page(sb);
    }

    private void DisplayCommands()
    {
        IEnumerable<KeyValuePair<string, IGameActionInfo>> filteredGameActions = Actor.GameActions.Where(x => !x.Value.Hidden);

        Func<string, bool> nameFilter;
        Func<string, bool> categoryFilter;

        var keyValuePairs = filteredGameActions as KeyValuePair<string, IGameActionInfo>[] ?? filteredGameActions.ToArray();
        if (Parameter.IsAll)
        {
            nameFilter = x => true;
            categoryFilter = x => true;
        }
        else
        {
            // if parameter match a category, display category
            string[] categories = keyValuePairs.SelectMany(x => x.Value.Categories).ToArray();
            var matchingCategory = categories.FirstOrDefault(x => StringCompareHelpers.StringEquals(x, Parameter.Value));
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
        StringBuilder sb = new();
        sb.AppendLine("Available commands:");
        foreach (var gameActionByCategory in keyValuePairs
            .SelectMany(x => x.Value.Names.Where(n => nameFilter(n)), (kv, name) => new { name, kv.Value })
            .GroupBy(x => x.name, (name, group) => new { name, group.First().Value })
            .SelectMany(x => x.Value.Categories.Where(categoryFilter), (kv, category) => new { category, kv.name, priority = kv.Value.Priority })
            .GroupBy(x => x.category, (category, group) => new { category, commands = group })
            .OrderBy(g => g.category))
        {
            if (!string.IsNullOrEmpty(gameActionByCategory.category))
                sb.AppendLine("%W%" + gameActionByCategory.category + ":%x%");
            int index = 0;
            foreach (var gameActionInfo in gameActionByCategory.commands
                .OrderBy(x => x.priority)
                .ThenBy(x => x.name))
            {
                sb.AppendFormat("{0,-14}", gameActionInfo.name);
                if ((++index % ColumnCount) == 0)
                    sb.AppendLine();
            }
            if (index > 0 && index % ColumnCount != 0)
                sb.AppendLine();
        }
        Actor.Page(sb);
    }
}
