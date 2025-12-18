using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
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
    public override void Execute(IActionInput actionInput)
    {
        Actor.Send("An admin cannot be deleted in game!!!");
    }
}
