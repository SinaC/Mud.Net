using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Effects.Interfaces;

public interface IEffect
{
}

public interface IEffect<in TEntity> : IEffect
    where TEntity : IEntity
{
    void Apply(TEntity target, IEntity source, string auraName, int level, int modifier);
}
