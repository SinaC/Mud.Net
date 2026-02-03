using Mud.Common;
using Mud.Domain;
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

namespace Mud.Server.Commands.PlayableCharacter.Quest;

[PlayableCharacterCommand("questinfo", "Quest", Priority = 6)]
[Alias("qinfo")]
[Syntax("[cmd]")]
public class QuestInfo : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new CannotBeInCombat(), new RequiresAtLeastOneArgument { Message = "What quest do you want info about ?" }];

    private IQuest What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var id = actionInput.Parameters[0].AsNumber;
        var quest = id > 0
            ? Actor.ActiveQuests.ElementAtOrDefault(id - 1)
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
