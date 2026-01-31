using Mud.Flags;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Commands.Player.Account;

[PlayerCommand("quit", "Account", Priority = 999/*low priority*/, NoShortcut = true)]
[Help(
@"[cmd] leaves the game.  You may [cmd] anywhere.  When you re-enter the game 
you will be back in the same room.

[cmd] automatically does a SAVE, so you can safely leave the game with just one
command.  Nevertheless it's a good idea to SAVE before [cmd].  If you get into
the habit of using [cmd] without SAVE, and then you play some other mud that
doesn't save before quitting, you're going to regret it.")]
public class Quit : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [new CannotBeInCombat()];

    private IServerPlayerCommand ServerPlayerCommand { get; }
    private IWiznet Wiznet { get; }

    public Quit(IServerPlayerCommand serverPlayerCommand, IWiznet wiznet)
    {
        ServerPlayerCommand = serverPlayerCommand;
        Wiznet = wiznet;
    }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Impersonating != null && Impersonating.ActiveQuests.OfType<IGeneratedQuest>().Any())
        {
            if (actionInput.Parameters.Length == 0 || actionInput.Parameters[0].Value != "quit")
                return "But you are still on an automated quest! use 'quit quit' to confirm";
        }

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Impersonating != null)
        {
            var generatedQuests = Impersonating.ActiveQuests.OfType<IGeneratedQuest>().ToArray();
            foreach (var generatedQuest in generatedQuests)
            {
                generatedQuest.Delete();
                Impersonating.RemoveQuest(generatedQuest);
            }
            if (generatedQuests.Length > 0)
                Impersonating.SetTimeLeftBeforeNextAutomaticQuest(TimeSpan.FromMinutes(15));
        }

        Actor.Send("Alas, all good things must come to an end.");
        Impersonating?.Act(ActOptions.ToRoom, "{0:N} has left the game.", Impersonating);
        Wiznet.Log($"{Actor.DisplayName} rejoins the real world.", new WiznetFlags("Logins"));

        ServerPlayerCommand.Quit(Actor);
    }
}
