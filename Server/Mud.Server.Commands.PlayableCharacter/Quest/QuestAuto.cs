using Mud.Blueprints.Character;
using Mud.Common;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Commands.PlayableCharacter.Quest;

[PlayableCharacterCommand("questauto", "Quest", Priority = 6)]
[Alias("qauto")]
[Syntax("[cmd]")]
public class QuestAuto : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new CannotBeInCombat()];

    private IQuestManager QuestManager { get; }
    private IRandomManager RandomManager { get; }

    public QuestAuto(IQuestManager questManager, IRandomManager randomManager)
    {
        QuestManager = questManager;
        RandomManager = randomManager;
    }

    private INonPlayableCharacter QuestGiver { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var (questGiver, _) = Actor.Room.GetNonPlayableCharacters<CharacterQuestorBlueprint>().FirstOrDefault();
        if (questGiver == null)
            return "You cannot get any quest here.";
        QuestGiver = questGiver;

        if (Actor.ActiveQuests.OfType<IGeneratedQuest>().Any())
            return Actor.ActPhrase("{0:N} tells you 'But you're already on a quest!'", QuestGiver);

        if (Actor.PulseLeftBeforeNextAutomaticQuest > 0)
            return Actor.ActPhrase("{0:N} tells you 'You're very brave, {1}, but let someone else have a chance.", QuestGiver, Actor.DisplayName);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        QuestGiverSays("Thank you, brave {1}", Actor.DisplayName);

        var quest = QuestManager.GenerateQuest(Actor, QuestGiver);
        if (quest == null)
        {
            QuestGiverSays("I'm sorry, but I don't have any quests for you at this time.");
            QuestGiverSays("Try again later.");
            Actor.SetTimeLeftBeforeNextAutomaticQuest(TimeSpan.FromMinutes(2));
            return;
        }

        switch (quest.GeneratedQuestType)
        {
            case GeneratedQuestType.FindItem:
                QuestGiverSays("Vile pilferes has stolen {1} from the royal treasury!", quest.ItemQuestBlueprint!.ShortDescription);
                QuestGiverSays("My court wizardess, with her magic mirror, has pinpointed its location.");
                QuestGiverSays("Look in the general area of {1} for {2}", quest.Room.Area.DisplayName, quest.ItemQuestBlueprint!.ShortDescription);
                break;
            case GeneratedQuestType.LootItem:
                QuestGiverSays("{1} has stolen {2} from the royal treasury!", quest.Target!.Blueprint.ShortDescription, quest.ItemQuestBlueprint!.ShortDescription);
                QuestGiverSays("My court wizardess, with her magic mirror, has pinpointed its location.");
                QuestGiverSays("Look in the general area of {1} for {2}", quest.Room.Area.DisplayName, quest.ItemQuestBlueprint!.ShortDescription);
                break;
            case GeneratedQuestType.KillMob:
                if (RandomManager.Chance(50))
                {
                    QuestGiverSays("An enemy of mine, {1} is making vile threats against the crown.", quest.Target!.Blueprint.ShortDescription);
                    QuestGiverSays("This threat must be eliminated!");
                }
                else
                {
                    QuestGiverSays("World's most heinous criminal {1} has escaped from the dungeon!", quest.Target!.Blueprint.ShortDescription);
                    QuestGiverSays("Since the escape, {1} has murdered {2} civillians!", quest.Target!.Blueprint.ShortDescription, 2 + RandomManager.Next(18));
                    QuestGiverSays("The penalty for this crime is death, and you are to deliver the sentence!");
                }
                QuestGiverSays("Seek {1} somewhere in the vicinity of {2}", quest.Target.Blueprint.ShortDescription, quest.Room.Name);
                QuestGiverSays("That location is in the general area of {1}", quest.Room.Area.DisplayName);
                break;
        }

        QuestGiverSays("You have {1} minutes to complete this quest", quest.TimeLimit);
        QuestGiverSays("May the gods go with you!");
    }

    private void QuestGiverSays(string phrase, params object[] args)
        => Actor.Act(ActOptions.ToCharacter, "{0:N} tells you '"+phrase+"'", QuestGiver.Yield().Concat(args ?? Enumerable.Empty<object>()).ToArray());
}
