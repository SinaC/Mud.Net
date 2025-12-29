using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Commands.Character.PlayableCharacter.Quest;

[PlayableCharacterCommand("questcomplete", "Quest", Priority = 2), MinPosition(Positions.Standing), NotInCombat]
[Alias("qcomplete")]
[Syntax(
       "[cmd] <id>",
       "[cmd] all")]
public class QuestComplete : PlayableCharacterGameAction
{
    protected IQuest[] What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Complete which quest?";
        // all
        if (actionInput.Parameters[0].IsAll)
        {
            What = Actor.Quests.Where(x => x.IsCompleted).ToArray();
            return null;
        }
        // id
        int id = actionInput.Parameters[0].AsNumber;
        IQuest quest = id > 0 
            ? Actor.Quests.ElementAtOrDefault(id - 1)!
            : null!;
        if (quest == null)
            return "No such quest.";
        if (Actor.Room.NonPlayableCharacters.All(x => x != quest.Giver))
            return $"You must be near {quest.Giver.DisplayName} to complete this quest.";
        if (!quest.IsCompleted)
            return $"Quest '{quest.Blueprint.Title}' is not finished!";
        What = [quest];
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        bool found = false;
        foreach (var questToComplete in What)
        {
            if (Actor.Room.NonPlayableCharacters.Any(x => x == questToComplete.Giver))
            {
                questToComplete.Complete();
                Actor.RemoveQuest(questToComplete);
                Actor.Send("You complete '{0}' successfully.", questToComplete.Blueprint.Title);
                found = true;
            }
        }
        if (!found)
            Actor.Send("No quest to complete here.");
    }
}
