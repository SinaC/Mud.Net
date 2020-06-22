using System.Linq;
using System.Text;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Information
{
    [CharacterCommand("equipment", "Information")]
    public class Equipment : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("You are using:");
            if (Actor.Equipments.All(x => x.Item == null))
                sb.AppendLine("Nothing");
            else
            {
                foreach (IEquippedItem equippedItem in Actor.Equipments)
                {
                    string where = equippedItem.EquipmentSlotsToString();
                    sb.Append(where);
                    if (equippedItem.Item == null)
                        sb.AppendLine("nothing");
                    else
                    {
                        equippedItem.Item.Append(sb, Actor, true);
                        sb.AppendLine();
                    }
                }
            }

            Actor.Send(sb);
        }
    }
}
