using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Commands.Player.Avatar;

[PlayerCommand("createavatar", "Avatar", CannotBeImpersonated = true)]
public class CreateAvatar : PlayerGameAction
{
    private ILogger<CreateAvatar> Logger { get; }
    private IServerPlayerCommand ServerPlayerCommand { get; }
    private IRaceManager RaceManager { get; }
    private IClassManager ClassManager { get; }
    private IUniquenessManager UniquenessManager { get; }
    private ITimeManager TimeManager { get; }
    private IRoomManager RoomManager { get; }
    private IFlagFactory<IShieldFlags, IShieldFlagValues> ShieldFlagFactory { get; }
    private IGameActionManager GameActionManager { get; }
    private int MaxAvatarCount { get; }

    public CreateAvatar(ILogger<CreateAvatar> logger, IServerPlayerCommand serverPlayerCommand, IRaceManager raceManager, IClassManager classManager, IUniquenessManager uniquenessManager, ITimeManager timeManager, IRoomManager roomManager, IFlagFactory<IShieldFlags, IShieldFlagValues> shieldFlagFactory, IGameActionManager gameActionManager, IOptions<AvatarOptions> avatarOptions)
    {
        Logger = logger;
        ServerPlayerCommand = serverPlayerCommand;
        RaceManager = raceManager;
        ClassManager = classManager;
        UniquenessManager = uniquenessManager;
        TimeManager = timeManager;
        RoomManager = roomManager;
        ShieldFlagFactory = shieldFlagFactory;
        GameActionManager = gameActionManager;
        MaxAvatarCount = avatarOptions.Value.MaxCount;
    }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Avatars.Count() >= MaxAvatarCount)
            return "Max. avatar count reached. Delete one before creating a new one.";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Send("Please choose an avatar name (type quit to stop and cancel creation).");
        Actor.SetStateMachine(new AvatarCreationStateMachine(Logger, ServerPlayerCommand, RaceManager, ClassManager, UniquenessManager, TimeManager, RoomManager, ShieldFlagFactory, GameActionManager));
    }
}
