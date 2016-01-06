using Mud.Server.Blueprints;

namespace Mud.Server
{
    public interface IItem : IEntity
    {
        IContainer ContainedInto { get; }

        ItemBlueprintBase Blueprint { get; }

        int Weight { get; }
        int Cost { get; }

        bool ChangeContainer(IContainer container);
    }
}
