using Mud.POC.Abilities2.ExistingCode;

namespace Mud.POC.Abilities2
{
    public interface IEffect<in TEntity>
        where TEntity : IEntity
    {
        void Apply(TEntity target, IEntity source, string auraName, int level, int modifier);
    }
}
