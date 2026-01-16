using Mud.Blueprints.Character;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Commands.Character.PlayableCharacter.Quest;

[PlayableCharacterCommand("questabandon", "Quest", Priority = 3), MinPosition(Positions.Standing), NotInCombat]
[Alias("qabandon")]
[Syntax("[cmd] <id>")]
public class QuestAbandon : PlayableCharacterGameAction
{
    protected IQuest What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Abandon which quest?";
        int id = actionInput.Parameters[0].AsNumber;
        What = id > 0 
            ? Actor.ActiveQuests.ElementAtOrDefault(id - 1)!
            : null!;
        if (What == null)
            return "No such quest.";

        if (What is IGeneratedQuest) // generated quest must be abandoned at quest master
        {
            var (questGiver, _) = Actor.Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>().FirstOrDefault(x => x.blueprint.Id == What.Giver.Blueprint.Id);
            if (questGiver == null)
                return "You cannot abandon that quest here.";
        }

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Send("You abandon '{0}'!", What.Title);
        What.Abandon();
        Actor.RemoveQuest(What);
    }
}
