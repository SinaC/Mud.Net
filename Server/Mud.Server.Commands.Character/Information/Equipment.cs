using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("equipment", "Information")]
[Help(@"[cmd] lists your equipment (armor, weapons, and held items).")]
public class Equipment : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [];

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new();
        sb.AppendLine("You are using:");
        if (Actor.Equipments.All(x => x.Item == null))
            sb.AppendLine("Nothing");
        else
        {
            // 2H weapon take mainhand + offhand if non giant size, we have to reduce number of visible offhand by the number of 2H weapon
            var countOffHandToHide = Actor.Size >= Sizes.Giant 
                ? 0
                : Actor.Equipments.Count(x => x.Slot == EquipmentSlots.MainHand && x.Item?.WearLocation == WearLocations.Wield2H);
            foreach (var equippedItem in Actor.Equipments)
            {
                // don't display offhand if they are used to wield a 2H weapon
                if (equippedItem.Item == null && equippedItem.Slot == EquipmentSlots.OffHand)
                {
                    if (countOffHandToHide-- > 0)
                        continue;
                }
                var where = equippedItem.EquipmentSlotsToString(Actor.Size);
                sb.Append(where);
                if (equippedItem.Item == null)
                    sb.AppendLine("nothing");
                else if (Actor.CanSee(equippedItem.Item))
                {
                    equippedItem.Item.Append(sb, Actor, true);
                    sb.AppendLine();
                }
                else
                    sb.AppendLine("something");
            }
        }

        Actor.Send(sb);
    }
}
