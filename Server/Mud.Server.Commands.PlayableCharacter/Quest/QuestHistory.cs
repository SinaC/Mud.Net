using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.TableGenerator;

namespace Mud.Server.Commands.PlayableCharacter.Quest;

[PlayableCharacterCommand("questhistory", "Quest", Priority = 6)]
[Alias("qhistory")]
[Syntax("[cmd]")]
public class QuestHistory : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new CannotBeInCombat()];

    public override void Execute(IActionInput actionInput)
    {
        if (Actor.CompletedQuests.Any())
        {
            var sb = TableGenerators.CompletedQuestTableGenerator.Value.Generate([
                    $"                 | Automatic | Non-automatic |",
                    $"                 +---------------------------+",
                    $"#Quest requested |       {Actor[AvatarStatisticTypes.GeneratedQuestsRequested],3} |           {Actor[AvatarStatisticTypes.PredefinedQuestsRequested],3} |",
                    $"#Quest completed |       {Actor[AvatarStatisticTypes.GeneratedQuestsCompleted],3} |           {Actor[AvatarStatisticTypes.PredefinedQuestsCompleted],3} |",
                    $"#Quest timed out |       {Actor[AvatarStatisticTypes.GeneratedQuestsTimedout],3} |           {Actor[AvatarStatisticTypes.PredefinedQuestsTimedout],3} |",
                    $"#Quest abandoned |       {Actor[AvatarStatisticTypes.GeneratedQuestsAbandoned],3} |           {Actor[AvatarStatisticTypes.PredefinedQuestsAbandoned],3} |"
                ], Actor.CompletedQuests);
            Actor.Page(sb);
        }
        else
            Actor.Send("No completed quests yet.");
    }
}
