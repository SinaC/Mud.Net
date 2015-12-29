using Mud.Server.Blueprints;

namespace Mud.Server
{
    public interface IObject : IEntity
    {
        IContainer ContainedInto { get; }

        ObjectBlueprint Blueprint { get; }

        bool ChangeContainer(IContainer container);
    }
}
