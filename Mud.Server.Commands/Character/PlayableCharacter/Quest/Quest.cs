using Mud.Blueprints.Quest;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
using System.Text;

namespace Mud.Server.Commands.Character.PlayableCharacter.Quest;

[PlayableCharacterCommand("quest", "Quest", Priority = 1), MinPosition(Positions.Standing), NotInCombat]
[Syntax(
        "[cmd]",
        "[cmd] <id>",
        "[cmd] auto",
        "[cmd] abandon <id>",
        "[cmd] complete <id>",
        "[cmd] complete all",
        "[cmd] get <quest name>",
        "[cmd] get all",
        "[cmd] list")]
public class Quest : PlayableCharacterGameAction
{
    private ICommandParser CommandParser { get; }
    private IGameActionManager GameActionManager { get; }

    public Quest(ICommandParser commandParser, IGameActionManager gameActionManager)
    {
        CommandParser = commandParser;
        GameActionManager = gameActionManager;
    }

    protected enum Actions
    {
        DisplayAll,
        Display,
        Auto,
        Abandon,
        Complete,
        Get,
        List
    }

    protected Actions Action { get; set; }
    protected IQuest What { get; set; } = default!;
    protected string CommandLine { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // no param -> quest info
        if (actionInput.Parameters.Length == 0)
        {
            Action = Actions.DisplayAll;
            return null;
        }

        // quest id
        if (actionInput.Parameters[0].IsNumber)
        {
            int id = actionInput.Parameters[0].AsNumber;
            What = id > 0
                ? Actor.Quests.ElementAtOrDefault(id - 1)!
                : null!; // index starts at 0
            if (What == null)
                return "No such quest.";
            Action = Actions.Display;
            return null;
        }

        // quest auto
        if ("auto".StartsWith(actionInput.Parameters[0].Value))
        {
            CommandLine = CommandParser.JoinParameters(actionInput.Parameters.Skip(1));
            Action = Actions.Auto;
            return null;
        }
        // quest abandon id
        if ("abandon".StartsWith(actionInput.Parameters[0].Value))
        {
            CommandLine = CommandParser.JoinParameters(actionInput.Parameters.Skip(1));
            Action = Actions.Abandon;
            return null;
        }

        // quest complete id
        if ("complete".StartsWith(actionInput.Parameters[0].Value))
        {
            CommandLine = CommandParser.JoinParameters(actionInput.Parameters.Skip(1));
            Action = Actions.Complete;
            return null;
        }

        // quest get all|title
        if ("get".StartsWith(actionInput.Parameters[0].Value))
        {
            CommandLine = CommandParser.JoinParameters(actionInput.Parameters.Skip(1));
            Action = Actions.Get;
            return null;
        }

        // quest list
        if ("list".StartsWith(actionInput.Parameters[0].Value))
        {
            CommandLine = CommandParser.JoinParameters(actionInput.Parameters.Skip(1));
            Action = Actions.List;
            return null;
        }

        return BuildCommandSyntax();
    }

    public override void Execute(IActionInput actionInput)
    {
        switch (Action)
        {
            case Actions.DisplayAll:
                {
                    StringBuilder sb = new ();
                    sb.AppendLine("Quests:");
                    if (!Actor.Quests.Any())
                        sb.AppendLine("None.");
                    else
                    {
                        int id = 0;
                        foreach (IQuest quest in Actor.Quests)
                        {
                            BuildQuestSummary(sb, quest, id);
                            id++;
                        }
                    }
                    if (!Actor.Quests.OfType<IGeneratedQuest>().Any())
                    {
                        if (Actor.PulseLeftBeforeNextAutomaticQuest > 0)
                        {
                            var secondsLeft = Actor.PulseLeftBeforeNextAutomaticQuest / Pulse.PulsePerSeconds;
                            sb.AppendFormatLine("Delay before next automated quest: {0}.", secondsLeft.FormatDelay());
                        }
                        else
                            sb.AppendFormatLine("Ready for an automated quest.");
                    }
                    Actor.Page(sb);
                    return;
                }
            case Actions.Display:
                {
                    StringBuilder sb = new ();
                    BuildQuestSummary(sb, What, null);
                    Actor.Page(sb);
                    return;
                }
            case Actions.Auto:
                {
                    var executionResults = GameActionManager.Execute<QuestAuto, IPlayableCharacter>(Actor, CommandLine);
                    if (executionResults != null)
                        Actor.Send(executionResults);
                    return;
                }
            case Actions.Abandon:
                {
                    var executionResults = GameActionManager.Execute<QuestAbandon, IPlayableCharacter>(Actor, CommandLine);
                    if (executionResults != null)
                        Actor.Send(executionResults);
                    return;
                }
            case Actions.Complete:
                {
                    var executionResults = GameActionManager.Execute<QuestComplete, IPlayableCharacter>(Actor, CommandLine);
                    if (executionResults != null)
                        Actor.Send(executionResults);
                    return;
                }
            case Actions.Get:
                {
                    var executionResults = GameActionManager.Execute<QuestGet, IPlayableCharacter>(Actor, CommandLine);
                    if (executionResults != null)
                        Actor.Send(executionResults);
                    return;
                }
            case Actions.List:
                {
                    var executionResults = GameActionManager.Execute<QuestList, IPlayableCharacter>(Actor, CommandLine);
                    if (executionResults != null)
                        Actor.Send(executionResults);
                    return;
                }
        }
    }

    private void BuildQuestSummary(StringBuilder sb, IQuest quest, int? id)
    {
        var questType = quest is IGeneratedQuest ? "[AUTO] " : string.Empty;
        var difficultyColor = StringHelpers.DifficultyColor(Actor.Level, quest.Level);
        // TODO: Table ?
        if (id.HasValue)
            sb.Append($"{id + 1,2}) {questType}{difficultyColor}{quest.Title}%x%: {(quest.AreObjectivesFulfilled ? "%g%complete%x%" : "in progress")}");
        else
            sb.Append($"{questType}{difficultyColor}{quest.Title}%x%: {(quest.AreObjectivesFulfilled ? "%g%complete%x%" : "in progress")}");
        if (quest.TimeLimit > 0)
            sb.Append($" Time left: {(quest.PulseLeft / Pulse.PulsePerSeconds).FormatDelay()}");
        sb.AppendLine();
        if (!quest.AreObjectivesFulfilled)
            BuildQuestObjectives(sb, quest);
    }

    private static void BuildQuestObjectives(StringBuilder sb, IQuest quest)
    {
        foreach (var objective in quest.Objectives)
        {
            // TODO: 2 columns ?
            if (objective.IsCompleted)
                sb.AppendLine($"     %g%{objective.CompletionState}%x%");
            else
                sb.AppendLine($"     {objective.CompletionState}");
        }
    }
}
