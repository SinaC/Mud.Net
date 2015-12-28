using System.Collections.Generic;
using Mud.Server.Blueprints;

namespace Mud.Server
{
    public interface IRoom : IEntity
    {
        RoomBlueprint Blueprint { get; }

        // TODO: exits, objects
        IReadOnlyCollection<ICharacter> CharactersInRoom { get; }
        IExit[] Exits { get; } // fixed length

        IExit Exit(ServerOptions.ExitDirections direction);

        void Enter(ICharacter character);
        void Leave(ICharacter character);
    }
}
