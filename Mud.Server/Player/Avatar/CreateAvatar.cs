using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Settings.Interfaces;

namespace Mud.Server.Player.Avatar;

[PlayerCommand("createavatar", "Avatar", CannotBeImpersonated = true)]
public class CreateAvatar : PlayerGameAction
{
    private IServerPlayerCommand ServerPlayerCommand { get; }
    private IRaceManager RaceManager { get; }
    private IClassManager ClassManager { get; }
    private IUniquenessManager UniquenessManager { get; }
    private ITimeManager TimeManager { get; }
    private IRoomManager RoomManager { get; }
    private IGameActionManager GameActionManager { get; }
    private ISettings Settings { get; }

    public CreateAvatar(IServerPlayerCommand serverPlayerCommand, IRaceManager raceManager, IClassManager classManager, IUniquenessManager uniquenessManager, ITimeManager timeManager, IRoomManager roomManager, IGameActionManager gameActionManager, ISettings settings)
    {
        ServerPlayerCommand = serverPlayerCommand;
        RaceManager = raceManager;
        ClassManager = classManager;
        UniquenessManager = uniquenessManager;
        TimeManager = timeManager;
        RoomManager = roomManager;
        GameActionManager = gameActionManager;
        Settings = settings;
    }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Avatars.Count() >= Settings.MaxAvatarCount)
            return "Max. avatar count reached. Delete one before creating a new one.";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Send("Please choose an avatar name (type quit to stop and cancel creation).");
        Actor.SetStateMachine(new AvatarCreationStateMachine(ServerPlayerCommand, RaceManager, ClassManager, UniquenessManager, TimeManager, RoomManager, GameActionManager));
    }
}
