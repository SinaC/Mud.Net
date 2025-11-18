using Mud.Common;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Actor;

[Command("help", Priority = 0)]
[Syntax(
    "[help] <topic>")]
[Help(
@"[cmd] without any arguments shows a one-page command summary.

[cmd] <keyword> shows a page of help on that keyword.  The keywords include
all the commands, spells, skills, passives, races and classes listed in the game.")]
public class Help : ActorGameAction
{
    private const int ColumnCount = 5;

    private bool ShouldDisplayCategoriesAndCommands { get; set; }
    protected ICommandParameter Parameter { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            ShouldDisplayCategoriesAndCommands = true;
        else
        {
            ShouldDisplayCategoriesAndCommands = false;
            var parameter = actionInput.Parameters[0];
            if (parameter.Value.Length < 2)
                return "topic must contain at least 2 characters";
            Parameter = parameter;
        }
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (ShouldDisplayCategoriesAndCommands)
        {
            var filteredGameActionInfos = Actor.GameActions.Where(x => !x.Value.Hidden && x.Value.Help != null).Select(x => x.Value);
            StringBuilder sb = new();
            sb.AppendLine("Available topics:");
            // commands by category
            foreach (var gameActionInfoByCategory in filteredGameActionInfos
                .SelectMany(x => x.Names, (gai, name) => new { name, gai })
                .GroupBy(x => x.name, (name, group) => new { name, group.First().gai })
                .SelectMany(x => x.gai.Categories, (kv, category) => new { category, kv.name, priority = kv.gai.Priority })
                .GroupBy(x => x.category, (category, group) => new { category, commands = group })
                .OrderBy(g => g.category))
            {
                if (!string.IsNullOrEmpty(gameActionInfoByCategory.category))
                    sb.AppendLine("%W%" + gameActionInfoByCategory.category + ":%x%");
                int index = 0;
                foreach (var gameActionInfo in gameActionInfoByCategory.commands
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
            // TODO: spells
            // TODO: skills
            // TODO: passives
            // TODO: races
            // TODO: classes
            // TODO: help not related to commands nor abilities nor races/classes
            Actor.Page(sb);
        }
        else
        {
            Func<string, bool> nameFilter = x => StringCompareHelpers.StringStartsWith(x, Parameter.Value);
            StringBuilder sb = new();
            // commands
            var filteredGameActionInfos = Actor.GameActions.Where(x => !x.Value.Hidden && x.Value.Help != null && nameFilter(x.Key)).Select(x => x.Value);
            foreach (var gameActionInfosByExecutionType in filteredGameActionInfos
                .GroupBy(x => x.CommandExecutionType) // avoid displaying multiple times the same commands
                .OrderBy(x => x.First().Name))
            {
                foreach (var gameActionInfo in gameActionInfosByExecutionType)
                {
                    var names = gameActionInfo.Names.ToArray();
                    var title = string.Join(", ", names.Select(x => $"%C%{x}%x%"));
                    sb.AppendLine(title);
                    var commandNames = string.Join("|", names);
                    var sbSyntax = BuildCommandSyntax(commandNames, gameActionInfo.Syntax, false);
                    sb.Append(sbSyntax);
                    sb.AppendLine();
                    var help = gameActionInfo.Help!.Replace("[cmd]", commandNames.ToUpperInvariant());
                    sb.AppendLine(help);
                }
            }
            // TODO: spells
            // TODO: skills
            // TODO: passives
            // TODO: races
            // TODO: classes
            // TODO: help not related to commands nor abilities nor races/classes
            Actor.Page(sb);
        }
    }
}
