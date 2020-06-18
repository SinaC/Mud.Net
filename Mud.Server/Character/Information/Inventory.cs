using System.Text;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Information
{
    [CharacterCommand("inventory", "Information")]
    public class Inventory : InformationCharacterGameActionBase
    {
        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("You are carrying:");
            AppendItems(sb, Actor.Inventory, true, true);
            Actor.Send(sb);
        }
    }
}
