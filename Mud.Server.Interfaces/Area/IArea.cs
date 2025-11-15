using Mud.Server.Blueprints.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Area;

public interface IArea
{
    Guid Id { get; }

    AreaBlueprint Blueprint { get; }

    string DisplayName { get; }
    string Builders { get; }
    string Credits { get; }

    IEnumerable<IRoom> Rooms { get; }
    IEnumerable<IPlayer> Players { get; }
    IEnumerable<ICharacter> Characters { get; }
    IEnumerable<IPlayableCharacter> PlayableCharacters { get; }

    void ResetArea();

    bool AddRoom(IRoom room);
    bool RemoveRoom(IRoom room);
}
