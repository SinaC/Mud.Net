using Mud.Server.Blueprints;

namespace Mud.Server
{
    public interface IItem : IEntity
    {
        IContainer ContainedInto { get; }

        ItemBlueprint Blueprint { get; }

        bool ChangeContainer(IContainer container);
    }
}
