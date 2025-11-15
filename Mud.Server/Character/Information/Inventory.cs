using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Character.Information;

[CharacterCommand("inventory", "Information")]
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
