using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Flags;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("ubermode", "Avatar", "Immortal"), MustBeImpersonated]
[Syntax("[cmd]")]
public class UberMode : AdminGameAction
{
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
