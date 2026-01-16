using Mud.Server.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Communication;

public abstract class TellGameActionBase : PlayerGameAction
{
    protected void InnerTell(IPlayer whom, string what) // used by DoTell and DoReply
    {
        string sentence = $"%g%{Actor.DisplayName} tells you '%G%{what}%g%'%x%";
        if (whom.IsAfk)
        {
            Actor.Send($"{whom.DisplayName} is AFK, but your tell will go through when {whom.DisplayName} returns.");
            whom.AddDelayedTell(sentence);
        }
        else
        {
            Actor.Send("%g%You tell {0}: '%G%{1}%g%'%x%", whom.DisplayName, what);
            whom.Send(sentence);
            whom.SetLastTeller(Actor);
        }
    }
}
