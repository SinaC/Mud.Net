using Mud.Common;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Quest.Objectives;
using System.Text;

namespace Mud.Server.Commands.Admin.Quest;

[AdminCommand("questdisplay", "Quest"), NoArgumentGuard]
[Syntax("[cmd] <character>")]
[Alias("qdisplay")]
public class QuestDisplay : AdminGameAction
{
    private ICharacterManager CharacterManager { get; }

    public QuestDisplay(ICharacterManager characterManager)
    {
        CharacterManager = characterManager;
    }

    protected IPlayableCharacter Whom { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Whom = FindHelpers.FindByName(CharacterManager.PlayableCharacters.Where(x => x.ImpersonatedBy != null), actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        if (!Whom.ActiveQuests.Any())
            return $"No quest to display on {Whom.DisplayName}";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new ();
        sb.AppendLine($"Quests for {Whom.DisplayName}:");
        int id = 0;
        foreach (var quest in Whom.ActiveQuests)
        {
            BuildQuestInfo(sb, quest, id);
            id++;
        }
        Actor.Page(sb);
    }

    //
    private static void BuildQuestInfo(StringBuilder sb, IQuest quest, int id)
    {
        sb.Append($"{id + 1,2}) {quest.Title}: {(quest.AreObjectivesFulfilled ? "%g%complete%x%" : "in progress")}");
        if (quest.TimeLimit > 0)
            sb.Append($" Time left : {Pulse.ToTimeSpan(quest.PulseLeft).FormatDelay()}");
        sb.AppendLine();
        foreach (var objective in quest.Objectives)
        {
            sb.Append($"     {objective.CompletionState}");
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
            sb.AppendLine();
        }
    }
}
