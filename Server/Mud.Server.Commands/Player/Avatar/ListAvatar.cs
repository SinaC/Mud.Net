using Mud.Common;
using Mud.Domain.SerializationData.Account;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
using Mud.Server.TableGenerator;
using System.Text;

namespace Mud.Server.Commands.Player.Avatar;

[PlayerCommand("listavatar", "Avatar")]
public class ListAvatar : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [];

    private IRoomManager RoomManager { get; }

    public ListAvatar(IRoomManager roomManager)
    {
        RoomManager = roomManager;
    }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (!Actor.AvatarMetaDatas.Any())
            return "You don't have any avatar available. Use createavatar to create one.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = AvatarMetaDataTableGenerator.Generate("Avatars", Actor.AvatarMetaDatas);
        Actor.Send(sb);
    }

    // Helpers
    private TableGenerator<AvatarMetaData> AvatarMetaDataTableGenerator 
    {
        get
        {
            var generator = new TableGenerator<AvatarMetaData>();
            generator.AddColumn("Name", 14, data => data.Name.UpperFirstLetter());
            generator.AddColumn("Level", 7, data => data.Level.ToString());
            generator.AddColumn("Class", 12, data => data.Class ?? "???");
            generator.AddColumn("Race", 12, data => data.Race ?? "???");
            generator.AddColumn("Location", 40, data => RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == data.RoomId)?.DisplayName ?? "In the void");
            return generator;
        } 
    }
}
