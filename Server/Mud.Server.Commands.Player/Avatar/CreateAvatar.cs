using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using Mud.Server.StateMachine.Interfaces;

namespace Mud.Server.Commands.Player.Avatar;

[PlayerCommand("createavatar", "Avatar")]
public class CreateAvatar : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [new CannotBeImpersonated()];

    private ILogger<CreateAvatar> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private int MaxAvatarCount { get; }

    public CreateAvatar(ILogger<CreateAvatar> logger, IServiceProvider serviceProvider)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
    }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.AvatarMetaDatas.Count() >= MaxAvatarCount)
            return "Max. avatar count reached. Delete one before creating a new one.";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var avatarCreationStateMachine = ServiceProvider.GetRequiredService<IAvatarCreationStateMachine>();
        if (avatarCreationStateMachine == null)
        {
            Logger.LogError("CreateAvatar: cannot create AvatarCreationStateMachine for {name}", Actor.DisplayName);
            Actor.Send(StringHelpers.SomethingGoesWrong);
            return;
        }
        avatarCreationStateMachine.Initialize(Actor);
        Actor.SetStateMachine(avatarCreationStateMachine);
    }
}
