using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Effect;

public interface IEffectManager
{
    int Count { get; }

    IEffect<TEntity>? CreateInstance<TEntity>(string effectName)
        where TEntity: IEntity;
}
