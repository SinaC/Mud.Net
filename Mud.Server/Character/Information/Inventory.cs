using System.Text;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Information
{
    [CharacterCommand("inventory", "Information")]
    public class Inventory : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("You are carrying:");
            ItemsHelpers.AppendItems(sb, Actor.Inventory, Actor, true, true);
            Actor.Send(sb);
        }
    }
}
