using Mud.Common;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using System.Text;

namespace Mud.Server.Commands.Actor;

[ActorCommand("commands", Priority = 0)]
[Alias("cmd")]
[Syntax(
        "[cmd]",
        "[cmd] all",
        "[cmd] <category>",
        "[cmd] <prefix>")]
[Help(
@"[cmd] shows you commands in a category or all the command
categories in the game.")]
public class Commands : ActorGameAction
{
    protected override IGuard<IActor>[] Guards => [];

    private const int ColumnCount = 5;

    private bool ShouldDisplayCategories { get; set; }
    private ICommandParameter Parameter { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
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
        IEnumerable<KeyValuePair<string, IGameActionInfo>> filteredGameActions = Actor.GameActions.Where(x => !x.Value.Hidden);

        StringBuilder sb = new();
        // display categories
        sb.AppendLine("Available categories:%W%");
        int index = 0;
        foreach (var category in filteredGameActions
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
        // display commands without categories
        sb.AppendLine("%x%No category:%W%");
        index = 0;
        foreach (var gameActionWithoutCategory in filteredGameActions
            .Where(x => x.Value.Categories.All(c => string.IsNullOrWhiteSpace(c)))
            .OrderBy(x => x.Key))
        {
            sb.AppendFormat("{0,-14}", gameActionWithoutCategory.Key);
            if ((++index % ColumnCount) == 0)
                sb.AppendLine();
        }
        if (index > 0 && index % ColumnCount != 0)
            sb.AppendLine();
        //
        sb.Append("%x%");
        Actor.Page(sb);
    }

    private void DisplayCommands()
    {
        Func<string, bool> nameFilter;
        Func<string, bool> categoryFilter;

        var filteredGameActions = Actor.GameActions.Where(x => !x.Value.Hidden).Select(x => x.Value);
        if (Parameter.IsAll)
        {
            nameFilter = x => true;
            categoryFilter = x => true;
        }
        else
        {
            // if parameter match a category, display category
            string[] categories = filteredGameActions.SelectMany(x => x.Categories).ToArray();
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
        foreach (var gameActionByCategory in filteredGameActions
            .SelectMany(x => x.Names.Where(n => nameFilter(n)), (gai, name) => new { name, gai })
            .GroupBy(x => x.name, (name, group) => new { name, group.First().gai })
            .SelectMany(x => x.gai.Categories.Where(categoryFilter), (kv, category) => new { category, kv.name, priority = kv.gai.Priority })
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
