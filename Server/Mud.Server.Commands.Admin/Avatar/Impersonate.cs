using Microsoft.Extensions.Options;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("impersonate", "Avatar", Priority = 0)]
[Syntax(
    "[cmd]",
    "[cmd] <avatar name>")]
public class Impersonate : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new CannotBeImpersonated()];

    private IServerPlayerCommand ServerPlayerCommand { get; }
    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IWiznet Wiznet { get; }
    private Player.Avatar.Impersonate PlayerImpersonate { get; }

    public Impersonate(IServerPlayerCommand serverPlayerCommand, IRoomManager roomManager, ICharacterManager characterManager, IOptions<WorldOptions> worldOptions, IWiznet wiznet)
    {
        ServerPlayerCommand = serverPlayerCommand;
        RoomManager = roomManager;
        CharacterManager = characterManager;
        Wiznet = wiznet;
        PlayerImpersonate = new(ServerPlayerCommand, RoomManager, CharacterManager, worldOptions, Wiznet);
    }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var guards = PlayerImpersonate.CanExecute(actionInput);
        if (guards != null)
            return guards;

        if (Actor.Incarnating != null)
            return $"Stop incarnating {Actor.Incarnating.DisplayName} before trying to impersonate.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        PlayerImpersonate.Execute(actionInput);
    }
}
