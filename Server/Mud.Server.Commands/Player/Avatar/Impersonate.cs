using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Domain;
using Mud.Domain.SerializationData.Account;
using Mud.Flags;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Commands.Player.Avatar;

[PlayerCommand("impersonate", "Avatar", Priority = 100)]
[Syntax(
    "[cmd]",
    "[cmd] <avatar name>")]
public class Impersonate : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [new CannotBeImpersonated(), new RequiresAtLeastOneArgument { Message = "Impersonate whom ?"}];

    private IServerPlayerCommand ServerPlayerCommand { get; }
    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IWiznet Wiznet { get; }
    private int DefaultRoomId { get; }

    public Impersonate(IServerPlayerCommand serverPlayerCommand, IRoomManager roomManager, ICharacterManager characterManager, IOptions<WorldOptions> worldOptions, IWiznet wiznet)
    {
        ServerPlayerCommand = serverPlayerCommand;
        RoomManager = roomManager;
        CharacterManager = characterManager;
        Wiznet = wiznet;
        DefaultRoomId = worldOptions.Value.BlueprintIds.DefaultRoom;
    }

    private AvatarMetaData Whom { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Whom = Actor.AvatarMetaDatas.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value))!;
        if (Whom == null)
            return "Avatar not found. Use 'listavatar' to display your avatar list.";

        if (Impersonating?.Name == Whom.Name)
            return $"You are already impersonating {Whom.Name}.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var location = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == Whom.RoomId);
        if (location == null)
        {
            location = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == DefaultRoomId)!;
            Wiznet.Log($"Invalid roomId {Whom.RoomId} for character {Whom.Name}!!", new WiznetFlags("Bugs"), AdminLevels.Implementor);
        }
        var avatarData = ServerPlayerCommand.LoadAvatar(Whom.Name);
        if (avatarData == null)
        {
            Wiznet.Log($"Avatar {Whom.Name} for player {Actor.Name} cannot be loaded!!", new WiznetFlags("Bugs"), AdminLevels.Implementor);
            return;
        }
        var avatar = CharacterManager.AddPlayableCharacter(Guid.NewGuid(), avatarData, Actor, location);
        Actor.Send("%M%You start impersonating %C%{0}%x%.", avatar.DisplayName);
        Actor.StartImpersonating(avatar);
    }
}
