using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Commands.Player.Avatar;

[PlayerCommand("stopimpersonate", "Avatar", Priority = 100), MustBeImpersonated]
[Syntax(
    "[cmd]")]
public class StopImpersonate : PlayerGameAction
{
    private IServerPlayerCommand ServerPlayerCommand { get; }

    public StopImpersonate(IServerPlayerCommand serverPlayerCommand)
    {
        ServerPlayerCommand = serverPlayerCommand;
    }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Impersonating.Fighting != null)
            return "Not while fighting!";

        if (Impersonating.Quests.OfType<IGeneratedQuest>().Any())
        {
            if (actionInput.Parameters.Length == 0 || actionInput.Parameters[0].Value != "stop")
                return "But you are still on an automated quest! use 'stopimpersonate stop' to confirm";
        }

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var generatedQuests = Impersonating.Quests.OfType<IGeneratedQuest>().ToArray();
        foreach (var generatedQuest in generatedQuests)
        {
            generatedQuest.Delete();
            Impersonating.RemoveQuest(generatedQuest);
        }
        if (generatedQuests.Length > 0)
            Impersonating.SetTimeLeftBeforeNextAutomaticQuest(TimeSpan.FromMinutes(15));

        Actor.Send("You stop impersonating {0}.", Impersonating.DisplayName);
        Actor.UpdateCharacterDataFromImpersonated();
        Actor.StopImpersonating();
        ServerPlayerCommand.Save(Actor);
    }
}
