using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Flags;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("ubermode", "Avatar", "Immortal")]
[Syntax("[cmd]")]
public class UberMode : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new MustBeImpersonated()];

    private IFlagsManager FlagsManager { get; }

    public UberMode(IFlagsManager flagsManager)
    {
        FlagsManager = flagsManager;
    }

    public override void Execute(IActionInput actionInput)
    {
        var availableFlags = FlagsManager.AvailableValues<IImmortalModes>();

        if (Impersonating.ImmortalMode.HasAll(availableFlags))
        {
            Actor.Send("%W%Immortal mode %y%UberMode%x% turned %R%OFF%x%");
            Impersonating.ChangeImmortalMode(new ImmortalModes());
        }
        else
        {
            Actor.Send("%W%Immortal mode %y%UberMode%x% turned %G%ON%x%");
            Impersonating.ChangeImmortalMode(new ImmortalModes(availableFlags.ToArray()));
        }
    }
}
