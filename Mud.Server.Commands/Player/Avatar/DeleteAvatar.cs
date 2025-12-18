using Mud.Common;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Player.Avatar;

[PlayerCommand("deleteavatar", "Avatar", CannotBeImpersonated = true, NoShortcut = true)]
[Syntax("[cmd] <avatar name>")]
public class DeleteAvatar : PlayerGameAction
{
    private IServerPlayerCommand ServerPlayerCommand { get; }
    private IUniquenessManager UniquenessManager { get; }

    public DeleteAvatar(IServerPlayerCommand serverPlayerCommand, IUniquenessManager uniquenessManager)
    {
        ServerPlayerCommand = serverPlayerCommand;
        UniquenessManager = uniquenessManager;
    }

    protected string AvatarName { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();

        var avatarName = actionInput.Parameters[0].Value;

        var avatarFound = Actor.Avatars.Any(x => StringCompareHelpers.StringEquals(x.Name, avatarName));
        if (!avatarFound)
        {
            Actor.ResetAvatarNameDeletionConfirmationNeeded();
            return "Avatar not found. Use 'listavatar' to display your avatar list.";
        }

        if (Actor.AvatarNameDeletionConfirmationNeeded != null && !StringCompareHelpers.StringEquals(Actor.AvatarNameDeletionConfirmationNeeded, avatarName))
        {
            Actor.ResetAvatarNameDeletionConfirmationNeeded();
            Actor.SetLag(5 * Pulse.PulsePerSeconds);
            return "Wrong avatar name confirmed. Wait 5 seconds.";
        }

        AvatarName = avatarName;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var avatar = Actor.Avatars.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, AvatarName));
        if (avatar == null)
        {
            Actor.Send("Something goes wrong!");
            return;
        }
        if (Actor.AvatarNameDeletionConfirmationNeeded == null)
        {
            Actor.Send("Ask you sure you want to delete avatar {0}? Use 'deleteavatar' {0} again to confirm.", avatar.Name);
            Actor.SetAvatarNameDeletionConfirmationNeeded(AvatarName);
            return;
        }

        Actor.Send("Avatar deletion confirmed! Processing...");
        Actor.DeleteAvatar(AvatarName);
        UniquenessManager.RemoveAvatarName(AvatarName);
        ServerPlayerCommand.Save(Actor);
        Actor.ResetAvatarNameDeletionConfirmationNeeded();
    }
}
