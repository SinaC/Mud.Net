using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("equipment", "Information")]
[Help(@"[cmd] lists your equipment (armor, weapons, and held items).")]
public class Equipment : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new();
        sb.AppendLine("You are using:");
        if (Actor.Equipments.All(x => x.Item == null))
            sb.AppendLine("Nothing");
        else
        {
            foreach (var equippedItem in Actor.Equipments)
            {
                var where = equippedItem.EquipmentSlotsToString();
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
