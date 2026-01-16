using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("inventory", "Information")]
[Help(@"[cmd] lists your inventory.")]
public class Inventory : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new ();
        sb.AppendLine("You are carrying:");
        ItemsHelpers.AppendItems(sb, Actor.Inventory, Actor, true, true);
        Actor.Send(sb);
    }
}
