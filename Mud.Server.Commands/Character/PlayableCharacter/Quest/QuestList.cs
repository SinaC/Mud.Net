using Mud.Blueprints.Character;
using Mud.Blueprints.Quest;
using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;
using System.Text;

namespace Mud.Server.Commands.Character.PlayableCharacter.Quest;

[PlayableCharacterCommand("questlist", "Quest", Priority = 5), MinPosition(Positions.Standing), NotInCombat]
[Alias("qlist")]
public class QuestList : PlayableCharacterGameAction
{
    protected List<QuestBlueprint> What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        What = [];
        var questGiverFound = false;
        var questAvailable = false;
        foreach (var (character, blueprint) in Actor.Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>())
        {
            if (blueprint?.QuestBlueprints?.Length > 0)
            {
                foreach (var questBlueprint in GetAvailableQuestBlueprints(blueprint))
                {
                    What.Add(questBlueprint);
                    questAvailable = true;
                }
                questGiverFound = true;
            }
        }

        if (!questGiverFound)
            return "You cannot get any quest here.";

        if (!questAvailable)
            return "No quest available for the moment.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new ();
        sb.AppendLine("Available quests:");
        foreach (var questBlueprint in What)
        {
            
            sb.AppendLine($"Quest '{StringHelpers.DifficultyColor(Actor.Level, questBlueprint.Level)}{questBlueprint.Title}%x%' [lvl:{questBlueprint.Level}]");
        }
        Actor.Page(sb);
    }

    private IEnumerable<QuestBlueprint> GetAvailableQuestBlueprints(CharacterQuestorBlueprint questGiverBlueprint)
        => questGiverBlueprint.QuestBlueprints.Where(x => Actor.ActiveQuests.OfType<IPredefinedQuest>().All(y => y.Blueprint.Id != x.Id) && Actor.CompletedQuests.All(y => y.QuestId != x.Id));
}
