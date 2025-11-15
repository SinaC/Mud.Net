using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Admin.Avatar;

[AdminCommand("immortal", "Avatar", MustBeImpersonated = true)]
[Syntax("[cmd]")]
public class Immortal : AdminGameAction
{
    public override void Execute(IActionInput actionInput)
    {

        if (Impersonating.IsImmortal)
        {
            Actor.Send("%R%BEWARE: %G%YOU ARE %R%MORTAL%G% NOW!%x%");
            Impersonating.ChangeImmortalState(false);
        }
        else
        {
            Actor.Send("%R%YOU ARE IMMORTAL NOW!%x%");
            Impersonating.ChangeImmortalState(true);
        }
    }
}
