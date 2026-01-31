using Mud.Common;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Avatar;

[PlayerCommand("deleteavatar", "Avatar", Priority = 1000, NoShortcut = true)]
[Syntax("[cmd] <avatar name>")]
public class DeleteAvatar : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [new CannotBeImpersonated(), new RequiresAtLeastOneArgument()];

    private IServerPlayerCommand ServerPlayerCommand { get; }

    public DeleteAvatar(IServerPlayerCommand serverPlayerCommand)
    {
        ServerPlayerCommand = serverPlayerCommand;
    }

    protected string AvatarName { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var avatarName = actionInput.Parameters[0].Value;

        var avatarMetaData = Actor.AvatarMetaDatas.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, avatarName));
        if (avatarMetaData == null)
        {
            Actor.ResetAvatarNameDeletionConfirmationNeeded();
            return "Avatar not found. Use 'listavatar' to display your avatar list.";
        }

        if (Actor.AvatarNameDeletionConfirmationNeeded != null && !StringCompareHelpers.StringEquals(Actor.AvatarNameDeletionConfirmationNeeded, avatarName))
        {
            Actor.ResetAvatarNameDeletionConfirmationNeeded();
            Actor.SetLag(Pulse.FromSeconds(5));
            return "Wrong avatar name confirmed. Wait 5 seconds.";
        }

        AvatarName = avatarMetaData.Name;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (Actor.AvatarNameDeletionConfirmationNeeded == null)
        {
            Actor.Send("Ask you sure you want to delete avatar {0}? Use 'deleteavatar' {0} again to confirm.", AvatarName);
            Actor.SetAvatarNameDeletionConfirmationNeeded(AvatarName);
            return;
        }

        Actor.Send("Avatar deletion confirmed! Processing...");
        Actor.DeleteAvatar(AvatarName);
        ServerPlayerCommand.DeleteAvatar(AvatarName);
        ServerPlayerCommand.Save(Actor);
        Actor.ResetAvatarNameDeletionConfirmationNeeded();
    }
}
