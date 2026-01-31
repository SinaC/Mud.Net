using Mud.Blueprints.Character;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Commands.Character.PlayableCharacter.Quest;

[PlayableCharacterCommand("questcomplete", "Quest", Priority = 2)]
[Alias("qcomplete")]
[Syntax(
       "[cmd] <id>",
       "[cmd] all")]
public class QuestComplete : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new CannotBeInCombat(), new RequiresAtLeastOneArgument { Message = "Complete which quest ?" }];

    private IQuest[] What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // all
        if (actionInput.Parameters[0].IsAll)
        {
            What = Actor.ActiveQuests.Where(x => x.AreObjectivesFulfilled).ToArray();
        }
        // id
        else
        {
            var id = actionInput.Parameters[0].AsNumber;
            var quest = id > 0
                ? Actor.ActiveQuests.ElementAtOrDefault(id - 1)
                : null;
            if (quest == null)
                return "No such quest.";
            if (Actor.Room.NonPlayableCharacters.All(x => x != quest.Giver))
                return $"You must be near {quest.Giver.DisplayName} to complete this quest.";
            if (!quest.AreObjectivesFulfilled)
                return $"Quest '{quest.Title}' is not finished!";
            What = [quest];
        }

        var generatedQuestsGuards = GuardsForGeneratedQuests();
        if (generatedQuestsGuards != null)
            return generatedQuestsGuards;

        return null;
    }

    private string? GuardsForGeneratedQuests()
    {
        if (What.OfType<IGeneratedQuest>().Any()) // generated quest must be abandoned at quest master
        {
            var firstGeneratedQuest = What.OfType<IGeneratedQuest>().First();
            var (questGiver, _) = Actor.Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>().FirstOrDefault(x => x.blueprint.Id == firstGeneratedQuest.Giver.Blueprint.Id);
            if (questGiver == null)
            {
                if (What.Length > 1)
                    return $"You cannot complete quest '{firstGeneratedQuest.Title}' here.";
                else
                    return $"You cannot complete that quest here.";
            }
        }
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var found = false;
        foreach (var questToComplete in What)
        {
            if (Actor.Room.NonPlayableCharacters.Any(x => x == questToComplete.Giver))
            {
                Actor.Send("You complete '{0}' successfully.", questToComplete.Title);
                questToComplete.Complete();
                if (questToComplete is IPredefinedQuest predefinedQuest)
                {
                    var completedQuest = predefinedQuest.GenerateCompletedQuest();
                    if (completedQuest != null)
                        Actor.AddCompletedQuest(completedQuest);
                }
                Actor.RemoveQuest(questToComplete);
                found = true;
            }
        }
        if (!found)
            Actor.Send("No quest to complete here.");
    }
}
