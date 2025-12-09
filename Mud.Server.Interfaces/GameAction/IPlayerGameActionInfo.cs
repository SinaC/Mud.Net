namespace Mud.Server.Interfaces.GameAction;

public interface IPlayerGameActionInfo : IActorGameActionInfo
{
    bool MustBeImpersonated { get; }
    bool CannotBeImpersonated { get; }
}
