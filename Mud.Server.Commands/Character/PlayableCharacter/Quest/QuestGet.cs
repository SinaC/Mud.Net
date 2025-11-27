using Mud.Common;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Quest;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Commands.Character.PlayableCharacter.Quest;

[PlayableCharacterCommand("questget", "Quest", Priority = 4, MinPosition = Positions.Standing, NotInCombat = true)]
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
        if (actionInput.Parameters[0].IsAll)
        {
            Func<QuestBlueprint, bool> filterFunc;
            if (!string.IsNullOrWhiteSpace(actionInput.Parameters[0].Value))
                filterFunc = x => StringCompareHelpers.StringStartsWith(x.Title, actionInput.Parameters[0].Value); 
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
                    if (StringCompareHelpers.StringStartsWith(questBlueprint.Title, actionInput.Parameters[0].Value))
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

    private IQuest GetQuest(QuestBlueprint questBlueprint, INonPlayableCharacter questGiver)
    {
        var quest = QuestManager.AddQuest(questBlueprint, Actor, questGiver);
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
