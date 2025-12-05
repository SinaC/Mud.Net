using Mud.Common;
using Mud.Domain.SerializationData;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.TableGenerator;
using System.Text;

namespace Mud.Server.Commands.Player.Avatar;

[PlayerCommand("listavatar", "Avatar")]
public class ListAvatar : PlayerGameAction
{
    private IClassManager ClassManager { get; }
    private IRaceManager RaceManager { get; }
    private IRoomManager RoomManager { get; }

    public ListAvatar(IClassManager classManager, IRaceManager raceManager, IRoomManager roomManager)
    {
        ClassManager = classManager;
        RaceManager = raceManager;
        RoomManager = roomManager;
    }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (!Actor.Avatars.Any())
            return "You don't have any avatar available. Use createavatar to create one.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = AvatarTableGenerator.Generate("Avatars", Actor.Avatars);
        Actor.Send(sb);
    }

    // Helpers
    private TableGenerator<PlayableCharacterData> AvatarTableGenerator 
    {
        get
        {
            TableGenerator<PlayableCharacterData> generator = new TableGenerator<PlayableCharacterData>();
            generator.AddColumn("Name", 14, data => data.Name.UpperFirstLetter());
            generator.AddColumn("Level", 7, data => data.Level.ToString());
            generator.AddColumn("Class", 12, data => ClassManager[data.Class]?.DisplayName ?? "none");
            generator.AddColumn("Race", 12, data => RaceManager[data.Race]?.DisplayName ?? "none");
            generator.AddColumn("Location", 40, data => RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == data.RoomId)?.DisplayName ?? "In the void");
            return generator;
        } 
    }
}
