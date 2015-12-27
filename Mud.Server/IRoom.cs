using System.Collections.Generic;

namespace Mud.Server
{
    public interface IRoom : IEntity
    {
        // TODO: exits, objects
        IReadOnlyCollection<ICharacter> CharactersInRoom { get; }
        IExit[] Exits { get; } // fixed length

        IExit Exit(ServerOptions.ExitDirections direction);

        void Enter(ICharacter character);
        void Leave(ICharacter character);
    }

    public interface IExit
    {
        string Keyword { get; }
        string Description { get; }
        // TODO: key pattern id or key pattern
        // TODO: flags
        IRoom Destination { get; }
    }
}
