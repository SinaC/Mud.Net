using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("ubermode", "Avatar", "Immortal"), MustBeImpersonated]
[Syntax("[cmd]")]
public class UberMode : AdminGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        if (Impersonating.ImmortalMode == ImmortalModeFlags.UberMode)
            Actor.Send("%W%Immortal mode %y%UberMode%x% turned %R%OFF%x%");
        else
            Actor.Send("%W%Immortal mode %y%UberMode%x% turned %G%ON%x%");
        Impersonating.ChangeImmortalMode(ImmortalModeFlags.UberMode);
    }
}
