using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.Room;

[Command("look", "Information")]
public class Look : GameActionBase<IRoom, IGameActionInfo>
{
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
