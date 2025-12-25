namespace Mud.Server.Interfaces.Combat;

public interface IMultiHitModifier : IHitModifier
{
    int MaxAttackCount { get; }
}
