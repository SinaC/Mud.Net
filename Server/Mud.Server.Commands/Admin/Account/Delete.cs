using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Account;

[AdminCommand("delete", "Account", Priority = 999, NoShortcut = true)]
[Alias("suicide")]
[Help(
@"Use the [cmd] command to erase unwanted characters, so the name will be
available for use.  This command must be typed twice to delete a character,
and you cannot be forced to delete.  Typing delete with an argument will
return your character to 'safe' status if you change your mind after the
first delete.")]
public class Delete : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [];

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        return "An admin cannot be deleted in game!!!";
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Send("An admin cannot be deleted in game!!!");
    }
}
