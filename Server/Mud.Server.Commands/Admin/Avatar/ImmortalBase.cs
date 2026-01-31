using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Admin.Avatar;

public abstract class ImmortalBase : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new MustBeImpersonated()];

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
