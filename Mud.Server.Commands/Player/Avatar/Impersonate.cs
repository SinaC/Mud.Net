using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Commands.Player.Avatar;

[PlayerCommand("impersonate", "Avatar", Priority = 100)]
[Syntax(
    "[cmd]",
    "[cmd] <avatar name>")]
public class Impersonate : PlayerGameAction
{
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

    protected PlayableCharacterData Whom { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Impersonating?.Fighting != null)
            return "Not while fighting!";

        if (actionInput.Parameters.Length == 0)
        {
            if (Impersonating == null)
                return "Impersonate whom?";
            return null;
        }

        Whom = Actor.Avatars.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value))!;
        if (Whom == null)
            return "Avatar not found. Use 'listavatar' to display your avatar list.";

        if (Impersonating?.Name == Whom.Name)
            return $"You are already impersonation {Whom.Name}.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Whom == null)
        {
            Actor.Send("You stop impersonating {0}.", Impersonating.DisplayName);
            Actor.UpdateCharacterDataFromImpersonated();
            Actor.StopImpersonating();
            ServerPlayerCommand.Save(Actor);
            return;
        }

        if (Impersonating != null)
        {
            Actor.UpdateCharacterDataFromImpersonated();
            Actor.StopImpersonating();
            ServerPlayerCommand.Save(Actor);
        }

        var location = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == Whom.RoomId);
        if (location == null)
        {
            location = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == DefaultRoomId)!;
            Wiznet.Log($"Invalid roomId {Whom.RoomId} for character {Whom.Name}!!", WiznetFlags.Bugs, AdminLevels.Implementor);
        }
        var avatar = CharacterManager.AddPlayableCharacter(Guid.NewGuid(), Whom, Actor, location);
        Actor.Send("%M%You start impersonating %C%{0}%x%.", avatar.DisplayName);
        Actor.StartImpersonating(avatar);
    }
}
