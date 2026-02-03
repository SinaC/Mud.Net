namespace Mud.Server.Ability.Passive.Interfaces;

public interface IAdditionalHitPassive : IPassive
{
    int AdditionalHitIndex { get; }
    bool StopMultiHitIfFailed { get; }
}
