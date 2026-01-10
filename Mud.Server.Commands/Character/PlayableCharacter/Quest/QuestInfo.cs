using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
using System.Text;

namespace Mud.Server.Commands.Character.PlayableCharacter.Quest;

[PlayableCharacterCommand("questinfo", "Quest", Priority = 6), MinPosition(Positions.Standing), NotInCombat]
[Alias("qinfo")]
[Syntax("[cmd]")]
public class QuestInfo : PlayableCharacterGameAction
{
    protected IQuest What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Info about which quest?";

        var id = actionInput.Parameters[0].AsNumber;
        var quest = id > 0
            ? Actor.Quests.ElementAtOrDefault(id - 1)
            : null;
        if (quest == null)
            return "No such quest.";

        What = quest;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var sb = new StringBuilder();
        var questType = What is IGeneratedQuest ? "[AUTO] " : string.Empty;
        var difficultyColor = StringHelpers.DifficultyColor(Actor.Level, What.Level);
        sb.Append($"{questType}{difficultyColor}{What.Title}%x%: {(What.AreObjectivesFulfilled ? "%g%complete%x%" : "in progress")}");
        if (What.TimeLimit > 0)
            sb.Append($" Time left: {Pulse.ToTimeSpan(What.PulseLeft).FormatDelay()}");
        sb.AppendLine();
        if (What.Description != null)
            sb.AppendLine(What.Description);
        BuildQuestObjectives(sb);
        Actor.Page(sb);
    }

    private void BuildQuestObjectives(StringBuilder sb)
    {
        foreach (var objective in What.Objectives)
        {
            // TODO: 2 columns ?
            if (objective.IsCompleted)
                sb.AppendLine($"     %g%{objective.CompletionState}%x%");
            else
                sb.AppendLine($"     {objective.CompletionState}");
        }
    }
}
