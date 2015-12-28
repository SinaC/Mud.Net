using Mud.Server.Blueprints;

namespace Mud.Server
{
    public interface IObject : IEntity
    {
        ObjectBlueprint Blueprint { get; }

        // TODO
    }
}
