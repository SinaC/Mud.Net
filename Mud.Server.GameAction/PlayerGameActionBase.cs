using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.GameAction
{
    public abstract class PlayerGameActionBase<TPlayer, TPlayerGameActionInfo> : GameActionBase<TPlayer, TPlayerGameActionInfo>
        where TPlayer : class, IPlayer
        where TPlayerGameActionInfo: class, IPlayerGameActionInfo
    {
        public IPlayableCharacter Impersonating { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            Impersonating = Actor.Impersonating;

            if (GameActionInfo.MustBeImpersonated && Actor.Impersonating == null)
                return $"You must be impersonated to use '{GameActionInfo.Name}'.";

            if (GameActionInfo.CannotBeImpersonated && Actor.Impersonating != null)
                return $"You cannot be impersonated to use '{GameActionInfo.Name}'.";

            if (Actor.IsAfk && GameActionInfo.Name.ToLowerInvariant() != "afk")
                Actor.ToggleAfk();

            if (GameActionInfo.Name.ToLowerInvariant() != "delete")
                // once another command then 'delete' is used, reset deletion confirmation
                Actor.ResetDeletionConfirmation();

            return null;
        }
    }
}
