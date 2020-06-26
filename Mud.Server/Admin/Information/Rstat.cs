using Mud.Common;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Room;
using System;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("rstat", "Information")]
    [Syntax("[cmd] <id>")]
    public class Rstat : AdminGameAction
    {
        private IRoomManager RoomManager { get; }

        public Rstat(IRoomManager roomManager)
        {
            RoomManager = roomManager;
        }

        public IRoom Room { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if(actionInput.Parameters.Length == 0 && Actor.Impersonating == null)
                return BuildCommandSyntax();

            if (actionInput.Parameters.Length >= 1 && !actionInput.Parameters[0].IsNumber)
                return BuildCommandSyntax();

            if (Actor.Impersonating != null)
                Room = Impersonating.Room;
            else
            {
                int id = actionInput.Parameters[0].AsNumber;
                Room = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == id);
            }
            if (Room == null)
                return "It doesn't exist.";
            if (Room.IsPrivate)
                return "That room is private right now.";
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            if (Room.Blueprint != null)
                sb.AppendFormatLine("Blueprint: {0}", Room.Blueprint.Id);
            else
                sb.AppendLine("No blueprint");
            sb.AppendFormatLine("Name: {0} Keywords: {1}", Room.Blueprint?.Name ?? "(none)", string.Join(",", Room.Keywords));
            sb.AppendFormatLine("DisplayName: {0}", Room.DisplayName);
            sb.AppendFormatLine("Description: {0}", Room.Description);
            sb.AppendFormatLine("Flags: {0} (base: {1})", Room.RoomFlags.Map(), Room.BaseRoomFlags.Map());
            sb.AppendFormatLine("Light: {0} Sector: {1} MaxSize: {2}", Room.Light, Room.SectorType, Room.MaxSize);
            sb.AppendFormatLine("Heal rate: {0} Resource rate: {1}", Room.HealRate, Room.ResourceRate);
            if (Room.ExtraDescriptions != null)
            {
                foreach (var lookup in Room.ExtraDescriptions)
                    foreach (string extraDescr in lookup)
                        sb.AppendFormatLine("ExtraDescription: {0} " + Environment.NewLine + "{1}", lookup.Key, extraDescr);
            }
            foreach (ExitDirections direction in EnumHelpers.GetValues<ExitDirections>())
            {
                IExit exit = Room[direction];
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
}
