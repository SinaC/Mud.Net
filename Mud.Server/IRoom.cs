using System.Collections.Generic;
using Mud.Server.Blueprints;
using Mud.Server.Server;

namespace Mud.Server
{
    public interface IRoom : IEntity, IContainer
    {
        RoomBlueprint Blueprint { get; }

        IReadOnlyCollection<ICharacter> People { get; }
        IExit[] Exits { get; } // fixed length

        IExit Exit(ServerOptions.ExitDirections direction);
        IRoom GetRoom(ServerOptions.ExitDirections direction);

        bool Enter(ICharacter character);
        bool Leave(ICharacter character);
    }
}
