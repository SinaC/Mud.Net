namespace Mud.Server.Interfaces.Ability;

public interface IAdditionalHitPassive : IPassive
{
    int AdditionalHitIndex { get; }
    bool StopMultiHitIfFailed { get; }
}
