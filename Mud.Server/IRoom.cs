using System.Collections.Generic;
using Mud.Server.Blueprints;
using Mud.Server.Constants;

namespace Mud.Server
{
    public interface IRoom : IEntity, IContainer
    {
        RoomBlueprint Blueprint { get; }

        IEnumerable<ICharacter> People { get; }
        IExit[] Exits { get; } // fixed length

        IExit Exit(ExitDirections direction);
        IRoom GetRoom(ExitDirections direction);

        bool Enter(ICharacter character);
        bool Leave(ICharacter character);
    }
}
