using Mud.Common;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.GameAction;

public abstract class PlayerGameActionBase<TPlayer, TPlayerGameActionInfo> : GameActionBase<TPlayer, TPlayerGameActionInfo>
    where TPlayer : class, IPlayer
    where TPlayerGameActionInfo: class, IPlayerGameActionInfo
{
    public IPlayableCharacter Impersonating { get; private set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Impersonating = Actor.Impersonating!;

        if (Actor.IsAfk && GameActionInfo.Name.ToLowerInvariant() != "afk")
            Actor.ToggleAfk();

        if (!StringCompareHelpers.StringEquals(GameActionInfo.Name, "delete"))
            // once another command then 'delete' is used, reset deletion confirmation
            Actor.ResetDeletionConfirmationNeeded();
        if (!StringCompareHelpers.StringEquals(GameActionInfo.Name, "deleteavatar"))
            // once another command then 'deleteavatar' is used, reset avatar deletion confirmation
            Actor.ResetAvatarNameDeletionConfirmationNeeded();

        return null;
    }
}
