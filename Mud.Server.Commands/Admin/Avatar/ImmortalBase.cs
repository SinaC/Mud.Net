using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Avatar;

public abstract class ImmortalBase : AdminGameAction
{
    protected abstract ImmortalModeFlags Flag { get; }

    public override void Execute(IActionInput actionInput)
    {
        if (Impersonating.ImmortalMode.HasFlag(Flag))
            Actor.Send("%W%Immortal mode %y%{0}%x% turned %R%OFF%x%", Flag);
        else
            Actor.Send("%W%Immortal mode %y%{0}%x% turned %G%ON%x%", Flag);
        Impersonating.ChangeImmortalMode(Impersonating.ImmortalMode ^ Flag);
    }
}
