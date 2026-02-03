using Mud.Common;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.POC.Commands.Room;

[RoomCommand("look", "POC")]
public class Look : RoomGameAction
{
    protected override IGuard<IRoom>[] Guards => [];

    public override void Execute(IActionInput actionInput)
    {
        //TODO: better 'UI'
        StringBuilder sb = new ();
        sb.AppendLine("People:");
        foreach (ICharacter character in Actor.People)
            sb.AppendFormatLine($"{character.DisplayName}");
        sb.AppendLine("Items:");
        foreach (IItem item in Actor.Content)
            sb.AppendFormatLine($"{item.DisplayName}");
        //
        Actor.Send(sb);
    }
}
