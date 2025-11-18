using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("slay", "Admin", Priority = 999, NoShortcut = true, MustBeImpersonated = true)]
[Syntax("[cmd] <character>")]
[Help(
@"[cmd] kills a character in cold blood, no saving throw.  Best not to use this
command on players if you enjoy being a god.")]
public class Slay : AdminGameAction
{
    private IWiznet Wiznet { get; }

    public Slay(IWiznet wiznet)
    {
        Wiznet = wiznet;
    }

    protected ICharacter Whom { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();

        Whom = FindHelpers.FindByName(Impersonating.Room.People, actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;
        if (Whom == Actor.Impersonating)
            return "Suicide is a mortal sin.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Wiznet.Log($"{Actor.DisplayName} slayed {Whom.DebugName}.", WiznetFlags.Punish);

        Whom.Act(ActOptions.ToAll, "{0:N} slay{0:v} {1} in cold blood!", Actor.Impersonating!, Whom);
        Whom.RawKilled(Actor.Impersonating, false);
    }
}
