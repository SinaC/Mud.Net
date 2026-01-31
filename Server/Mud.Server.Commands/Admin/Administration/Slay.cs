using Mud.Flags;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("slay", "Admin", Priority = 999, NoShortcut = true)]
[Syntax("[cmd] <character>")]
[Help(
@"[cmd] kills a character in cold blood, no saving throw.  Best not to use this
command on players if you enjoy being a god.")]
public class Slay : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new MustBeImpersonated(), new RequiresAtLeastOneArgument()];

    private IWiznet Wiznet { get; }

    public Slay(IWiznet wiznet)
    {
        Wiznet = wiznet;
    }

    private ICharacter Whom { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Whom = FindHelpers.FindByName(Impersonating.Room.People, actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;
        if (Whom == Actor.Impersonating)
            return "Suicide is a mortal sin.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Wiznet.Log($"{Actor.DisplayName} slayed {Whom.DebugName}.", new WiznetFlags("Punish"));

        Whom.Act(ActOptions.ToAll, "%R%{0:N} slay{0:v} {1} in cold blood!%x%", Actor.Impersonating!, Whom);
        Whom.RawKilled(Actor.Impersonating, false);
    }
}
