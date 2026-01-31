using Mud.Blueprints.Room;
using Mud.Common;
using Mud.Server.Common.Extensions;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("rinfo", "Information")]
[Syntax("[cmd] <id>")]
public class Rinfo : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [];

    private IRoomManager RoomManager { get; }

    public Rinfo(IRoomManager roomManager)
    {
        RoomManager = roomManager;
    }

    private RoomBlueprint Blueprint { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0 && Actor.Impersonating == null)
            return BuildCommandSyntax();

        if (actionInput.Parameters.Length >= 1 && !actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();

        if (actionInput.Parameters.Length >= 1)
        {
            int id = actionInput.Parameters[0].AsNumber;
            Blueprint = RoomManager.GetRoomBlueprint(id)!;
        }
        else if (Actor.Impersonating != null)
            Blueprint = Actor.Impersonating.Room.Blueprint;

        if (Blueprint == null)
            return "Not found.";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new ();
        sb.AppendFormatLine("Blueprint: {0}", Blueprint.Id);
        sb.AppendFormatLine("Name: {0} AreaId: {1}", Blueprint.Name ?? "(none)", Blueprint.AreaId);
        sb.AppendFormatLine("Flags: {0}", Blueprint.RoomFlags);
        sb.AppendFormatLine("Sector: {0} MaxSize: {1}", Blueprint.SectorType, Blueprint.MaxSize?.ToString() ?? "NoSize");
        sb.AppendFormatLine("Heal rate: {0}% Resource rate: {1}%", Blueprint.HealRate, Blueprint.ResourceRate);
        sb.AppendFormat("Description: {0}", Blueprint.Description);
        if (Blueprint.ExtraDescriptions != null)
        {
            foreach (var extraDescription in Blueprint.ExtraDescriptions)
            {
                sb.AppendFormatLine("ExtraDescription: {0}", string.Join(",", extraDescription.Keywords));
                sb.Append(extraDescription.Description);
            }
        }
        foreach (var exitBlueprint in Blueprint.Exits.Where(x => x != null))
        {
            sb.AppendFormatLine("Exit: {0} - {1} Key:{2} Flags:{3}", exitBlueprint.Direction.DisplayName(), exitBlueprint.Destination, exitBlueprint.Key, exitBlueprint.Flags);
            if (string.IsNullOrEmpty(exitBlueprint.Description))
                sb.AppendFormatLine("Keyword:'{0}' Description:/", exitBlueprint.Keyword);
            else
                sb.AppendFormat("Keyword:'{0}' Description:{1}", exitBlueprint.Keyword, exitBlueprint.Description);
        }
        Actor.Send(sb);
    }
}
