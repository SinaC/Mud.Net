using Mud.Domain;
using Mud.Blueprints.Character;
using Mud.Blueprints.Quest;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Character.PlayableCharacter.Quest;

[PlayableCharacterCommand("questlist", "Quest", Priority = 5, MinPosition = Positions.Standing, NotInCombat = true)]
[Alias("qlist")]
public class QuestList : PlayableCharacterGameAction
{
    protected List<QuestBlueprint> What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        What = new List<QuestBlueprint>();
        bool questGiverFound = false;
        bool questAvailable = false;
        foreach (var (character, blueprint) in Actor.Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>())
        {
            if (blueprint?.QuestBlueprints?.Length > 0)
            {
                foreach (QuestBlueprint questBlueprint in GetAvailableQuestBlueprints(blueprint))
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
        foreach (QuestBlueprint questBlueprint in What)
            sb.AppendLine($"Quest '{questBlueprint.Title}' [lvl:{questBlueprint.Level}]");
        Actor.Page(sb);
    }

    private IEnumerable<QuestBlueprint> GetAvailableQuestBlueprints(CharacterQuestorBlueprint questGiverBlueprint) => questGiverBlueprint.QuestBlueprints.Where(x => Actor.Quests.All(y => y.Blueprint.Id != x.Id));
}
