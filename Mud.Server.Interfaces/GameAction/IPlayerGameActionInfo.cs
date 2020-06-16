namespace Mud.Server.Interfaces.GameAction
{
    public interface IPlayerGameActionInfo : IGameActionInfo
    {
        bool MustBeImpersonated { get; }
        bool CannotBeImpersonated { get; }
    }
}
