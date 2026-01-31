using Mud.Blueprints.Character;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Commands.Character.PlayableCharacter.Quest;

[PlayableCharacterCommand("questabandon", "Quest", Priority = 3)]
[Alias("qabandon")]
[Syntax("[cmd] <id>")]
public class QuestAbandon : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new CannotBeInCombat(), new RequiresAtLeastOneArgument { Message = "Abandan which quest ?" }];

    private IQuest What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

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
