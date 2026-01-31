using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.POC.Commands.Item;

[ItemCommand("look", "POC")]
public class Look : ItemGameActionBase<IItem, IItemGameActionInfo>
{
    protected override IGuard<IItem>[] Guards => [];

    public override void Execute(IActionInput actionInput)
    {
        //TODO: better 'UI'
        StringBuilder sb = new();

        var container = Actor.EquippedBy ?? Actor.ContainedInto;

        sb.AppendLine($"You are in {container.DisplayName}");

        if (container is IRoom room)
        {
            sb.AppendLine(room.Description);
            sb.AppendLine("People:");
            foreach (ICharacter character in room.People)
                sb.AppendFormatLine($"{character.DisplayName}");
        }
        if (container is IContainer containedInto)
        {
            sb.AppendLine("Items:");
            foreach (IItem item in containedInto.Content)
                sb.AppendFormatLine($"{item.DisplayName}");
        }

        //
        Actor.Send(sb);
    }
}
