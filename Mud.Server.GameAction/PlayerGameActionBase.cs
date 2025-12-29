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

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Impersonating = Actor.Impersonating!;

        if (GameActionInfo.PlayerGuards.Length > 0)
        {
            foreach (var guard in GameActionInfo.PlayerGuards)
            {
                var guardResult = guard.Guards(Actor);
                if (guardResult != null)
                    return guardResult;
            }
        }

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
