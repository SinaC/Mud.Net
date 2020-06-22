using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Effect
{
    public interface IEffectManager
    {
        IEffect<TEntity> CreateInstance<TEntity>(string name)
            where TEntity: IEntity;
    }
}
