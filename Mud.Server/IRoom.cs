using System.Collections.Generic;
using Mud.Server.Blueprints;

namespace Mud.Server
{
    public interface IRoom : IEntity, IContainer
    {
        RoomBlueprint Blueprint { get; }

        IReadOnlyCollection<ICharacter> People { get; }
        IExit[] Exits { get; } // fixed length

        IExit Exit(ServerOptions.ExitDirections direction);
        void Enter(ICharacter character);
        void Leave(ICharacter character);
    }
}
