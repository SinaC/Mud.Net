using Mud.Common;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("rstat", "Information")]
[Syntax("[cmd] <id>")]
public class Rstat : AdminGameAction
{
    private IRoomManager RoomManager { get; }

    public Rstat(IRoomManager roomManager)
    {
        RoomManager = roomManager;
    }

    protected IRoom Room { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if(actionInput.Parameters.Length == 0 && Actor.Impersonating == null)
            return BuildCommandSyntax();

        if (actionInput.Parameters.Length >= 1 && !actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();

        if (actionInput.Parameters.Length >= 1)
        {
            int id = actionInput.Parameters[0].AsNumber;
            Room = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == id)!;
        }
        else if (Actor.Impersonating != null)
            Room = Impersonating.Room!;

        if (Room == null)
            return "It doesn't exist.";
        if (Room.IsPrivate)
            return "That room is private right now.";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new ();
        sb.AppendFormatLine("Blueprint: {0}", Room.Blueprint.Id);
        sb.AppendFormatLine("Name: {0} Keywords: {1}", Room.Blueprint.Name ?? "(none)", string.Join(",", Room.Keywords));
        sb.AppendFormatLine("DisplayName: {0}", Room.DisplayName);
        sb.AppendFormatLine("Description: {0}", Room.Description);
        sb.AppendFormatLine("Flags: {0} (base: {1})", Room.RoomFlags, Room.BaseRoomFlags);
        sb.AppendFormatLine("Light: {0} Sector: {1} MaxSize: {2}", Room.Light, Room.SectorType, Room.MaxSize?.ToString() ?? "NoSize");
        sb.AppendFormatLine("Heal rate: {0}% (base {1}%) Resource rate: {2}% (base {3}%)", Room.HealRate, Room.BaseHealRate, Room.ResourceRate, Room.BaseResourceRate);
        if (Room.ExtraDescriptions != null)
        {
            foreach (var lookup in Room.ExtraDescriptions)
                foreach (string extraDescr in lookup)
                    sb.AppendFormatLine("ExtraDescription: {0} " + Environment.NewLine + "{1}", lookup.Key, extraDescr);
        }
        foreach (ExitDirections direction in Enum.GetValues<ExitDirections>())
        {
            var exit = Room[direction];
            if (exit?.Destination != null)
            {
                sb.Append(direction.DisplayName());
                sb.Append(" - ");
                sb.Append(exit.Destination.DisplayName);
                if (exit.IsClosed)
                    sb.Append(" (CLOSED)");
                if (exit.IsHidden)
                    sb.Append(" [HIDDEN]");
                if (exit.IsLocked)
                    sb.AppendFormat(" <Locked> {0}", exit.Blueprint.Key);
                sb.Append($" [{exit.Destination.Blueprint?.Id.ToString() ?? "???"}]");
                sb.AppendLine();
            }
            // TODO: exits
            // TODO: content
            // TODO: people
        }
        foreach (IAura aura in Room.Auras)
            aura.Append(sb);
        Actor.Send(sb);
    }
}
