using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Commands.Player.Avatar;

[PlayerCommand("unimpersonate", "Avatar", Priority = 100)]
[Syntax(
    "[cmd]")]
public class UnImpersonate : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [new MustBeImpersonated()];

    private IServerPlayerCommand ServerPlayerCommand { get; }

    public UnImpersonate(IServerPlayerCommand serverPlayerCommand)
    {
        ServerPlayerCommand = serverPlayerCommand;
    }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Impersonating.Fighting != null)
            return "Not while fighting!";

        if (Impersonating.ActiveQuests.OfType<IGeneratedQuest>().Any())
        {
            if (actionInput.Parameters.Length == 0 || actionInput.Parameters[0].Value != "stop")
                return "But you are still on an automated quest! use 'unimpersonate stop' to confirm";
        }

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var generatedQuests = Impersonating.ActiveQuests.OfType<IGeneratedQuest>().ToArray();
        foreach (var generatedQuest in generatedQuests)
        {
            generatedQuest.Delete();
            Impersonating.RemoveQuest(generatedQuest);
        }
        if (generatedQuests.Length > 0)
            Impersonating.SetTimeLeftBeforeNextAutomaticQuest(TimeSpan.FromMinutes(15));

        Actor.Send("You stop impersonating {0}.", Impersonating.DisplayName);
        ServerPlayerCommand.Save(Actor); // save player
        Actor.StopImpersonating();
    }
}
