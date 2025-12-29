using Mud.Common;
using Mud.Domain;
using Mud.Blueprints.Character;
using Mud.Blueprints.Quest;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Guards.Attributes;

namespace Mud.Server.Commands.Character.PlayableCharacter.Quest;

[PlayableCharacterCommand("questget", "Quest", Priority = 4), MinPosition(Positions.Standing), NotInCombat]
[Alias("qget")]
[Syntax(
        "[cmd] <quest name>",
        "[cmd] all",
        "[cmd] all.<quest name>")]
public class QuestGet : PlayableCharacterGameAction
{
    private IQuestManager QuestManager { get; }

    public QuestGet(IQuestManager questManager)
    {
        QuestManager = questManager;
    }

    protected List<(QuestBlueprint questBlueprint, INonPlayableCharacter questGiver)> What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Get which quest?";

        if (!Actor.Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>().Any())
            return "You cannot get any quest here.";

        What = [];

        // get all
        var whatParameter = actionInput.Parameters[0];
        if (whatParameter.IsAll)
        {
            Func<QuestBlueprint, bool> filterFunc;
            if (!whatParameter.IsAllOnly)
                filterFunc = x => StringCompareHelpers.StringStartsWith(x.Title, whatParameter.Value); 
            else
                filterFunc = _ => true;

            foreach (var (character, blueprint) in Actor.Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>())
            {
                if (blueprint?.QuestBlueprints?.Length > 0)
                {
                    foreach (QuestBlueprint questBlueprint in GetAvailableQuestBlueprints(blueprint).Where(filterFunc))
                        What.Add((questBlueprint, character));
                }
            }
            if (What.Count == 0)
                Actor.Send("You are already running all available quests here.");
            return null;
        }

        // get quest (every quest starting with parameter)
        // Search quest giver with wanted quests
        foreach (var (character, blueprint) in Actor.Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>())
        {
            if (blueprint?.QuestBlueprints?.Length > 0)
            {
                foreach (QuestBlueprint questBlueprint in GetAvailableQuestBlueprints(blueprint))
                {
                    if (StringCompareHelpers.StringStartsWith(questBlueprint.Title, whatParameter.Value))
                        What.Add((questBlueprint, character));
                }
            }
        }
        if (What.Count == 0)
            return "No such quest can be get here.";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (var (questBlueprint, questGiver) in What)
            GetQuest(questBlueprint, questGiver);
    }

    private IQuest? GetQuest(QuestBlueprint questBlueprint, INonPlayableCharacter questGiver)
    {
        var quest = QuestManager.AddQuest(questBlueprint, Actor, questGiver);
        if (quest == null)
            return null;
        //
        Actor.Act(ActOptions.ToRoom, "{0} get{0:v} quest '{1}'.", Actor, questBlueprint.Title);
        if (questBlueprint.TimeLimit > 0)
            Actor.Send($"You get quest '{questBlueprint.Title}'. Better hurry, you have {questBlueprint.TimeLimit} minutes to complete this quest!");
        else
            Actor.Send($"You get quest '{questBlueprint.Title}'.");
        return quest;
    }

    private IEnumerable<QuestBlueprint> GetAvailableQuestBlueprints(CharacterQuestorBlueprint questGiverBlueprint) => questGiverBlueprint.QuestBlueprints.Where(x => Actor.Quests.All(y => y.Blueprint.Id != x.Id));
}
