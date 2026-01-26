using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Avatar;

public abstract class ImmortalBase : AdminGameAction
{
    protected abstract string Flag { get; }

    public override void Execute(IActionInput actionInput)
    {
        var immortalMode = Impersonating.ImmortalMode;
        if (immortalMode.IsSet(Flag))
        {
            Actor.Send("%W%Immortal mode %y%{0}%x% turned %R%OFF%x%", Flag);
            immortalMode.Unset(Flag);
        }
        else
        {
            Actor.Send("%W%Immortal mode %y%{0}%x% turned %G%ON%x%", Flag);
            immortalMode.Set(Flag);
        }
        Impersonating.ChangeImmortalMode(immortalMode);
    }
}
