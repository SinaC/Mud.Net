using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Player.Avatar;

[PlayerCommand("createavatar", "Avatar"), CannotBeImpersonated]
public class CreateAvatar : PlayerGameAction
{
    private ILogger<CreateAvatar> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private int MaxAvatarCount { get; }

    public CreateAvatar(ILogger<CreateAvatar> logger, IServiceProvider serviceProvider)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
    }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.AvatarMetaDatas.Count() >= MaxAvatarCount)
            return "Max. avatar count reached. Delete one before creating a new one.";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var avatarCreationStateMachine = ServiceProvider.GetRequiredService<AvatarCreationStateMachine>();
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
