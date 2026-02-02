using Mud.Common;
using Mud.Domain;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Common;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Quest.Objectives;
using System.Text;

namespace Mud.Server.Commands.Character.PlayableCharacter.Quest;

[PlayableCharacterCommand("quest", "Quest", Priority = 1)]
[Syntax(
        "[cmd]",
        "[cmd] <id>",
        "[cmd] auto",
        "[cmd] abandon <id>",
        "[cmd] complete <id>",
        "[cmd] complete all",
        "[cmd] get <quest name>",
        "[cmd] get all",
        "[cmd] info",
        "[cmd] list",
        "[cmd] history")]
public class Quest : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new CannotBeInCombat()];

    private ICommandParser CommandParser { get; }
    private IGameActionManager GameActionManager { get; }

    public Quest(ICommandParser commandParser, IGameActionManager gameActionManager)
    {
        CommandParser = commandParser;
        GameActionManager = gameActionManager;
    }

    private enum Actions
    {
        DisplayAll,
        Display,
        SubCommand
    }

    private (string? parameter, Type? GameActionType)[] ActionTable { get; } =
    [
        ("auto", typeof(QuestAuto)),
        ("abandon", typeof(QuestAbandon)),
        ("complete", typeof(QuestComplete)),
        ("get", typeof(QuestGet)),
        ("info", typeof(QuestInfo)),
        ("list", typeof(QuestList)),
        ("history", typeof(QuestHistory)),
    ];

    private Actions Action { get; set; }
    private Type? GameActionType { get; set; }
    private IQuest? What { get; set; }
    private string? CommandLine { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
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
                ? Actor.ActiveQuests.ElementAtOrDefault(id - 1)!
                : null!; // index starts at 0
            if (What == null)
                return "No such quest.";
            Action = Actions.Display;
            return null;
        }

        // search in action table
        foreach (var actionTableEntry in ActionTable.Where(x => x.parameter is not null && x.GameActionType is not null))
        {
            if (actionTableEntry.parameter!.StartsWith(actionInput.Parameters[0].Value))
            {
                Action = Actions.SubCommand;
                GameActionType = actionTableEntry.GameActionType;
                CommandLine = CommandParser.JoinParameters(actionInput.Parameters.Skip(1));
                return null;
            }
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
                    if (!Actor.ActiveQuests.Any())
                        sb.AppendLine("None.");
                    else
                    {
                        int id = 0;
                        foreach (IQuest quest in Actor.ActiveQuests)
                        {
                            BuildQuestSummary(sb, quest, id);
                            id++;
                        }
                    }
                    if (!Actor.ActiveQuests.OfType<IGeneratedQuest>().Any())
                    {
                        if (Actor.PulseLeftBeforeNextAutomaticQuest > 0)
                        {
                            var timeLeft = Pulse.ToTimeSpan(Actor.PulseLeftBeforeNextAutomaticQuest);
                            sb.AppendFormatLine("Delay before next automated quest: {0}.", timeLeft.FormatDelay());
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
                    BuildQuestSummary(sb, What!, null);
                    Actor.Page(sb);
                    return;
                }
            case Actions.SubCommand:
                var executionResults = GameActionManager.Execute(GameActionType!, Actor, CommandLine);
                if (executionResults != null)
                    Actor.Send(executionResults);
                return;
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
            sb.Append($" Time left: {Pulse.ToTimeSpan(quest.PulseLeft).FormatDelay()}");
        sb.AppendLine();
        if (!quest.AreObjectivesFulfilled)
            BuildQuestObjectives(sb, quest);
    }

    private void BuildQuestObjectives(StringBuilder sb, IQuest quest)
    {
        foreach (var objective in quest.Objectives)
        {
            // TODO: 2 columns ?
            if (objective.IsCompleted)
                sb.Append($"     %g%{objective.CompletionState}%x%");
            else
                sb.Append($"     {objective.CompletionState}");
            if (Actor.ImmortalMode.IsSet("Holylight"))
            {
                switch (objective)
                {
                    case LocationQuestObjective locationQuestObjective:
                        sb.Append($" {locationQuestObjective.RoomBlueprint.Name}[{locationQuestObjective.RoomBlueprint.Id}]");
                        break;
                    case FloorItemQuestObjective floorItemQuestObjective:
                        sb.Append($" [{string.Join(',', floorItemQuestObjective.RoomBlueprintIds)}]");
                        break;
                    case KillQuestObjective killQuestObjective:
                        sb.Append($" {killQuestObjective.TargetBlueprint.Name}[{killQuestObjective.TargetBlueprint.Id}]");
                        break;
                }
            }
            sb.AppendLine();
        }
    }
}
