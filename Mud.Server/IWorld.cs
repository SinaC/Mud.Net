using System.Collections.Generic;

namespace Mud.Server
{
    public interface IWorld
    {
        IPlayer GetPlayer(string name);
        IPlayer GetPlayer(CommandParameter parameter);
        IReadOnlyCollection<IPlayer> GetPlayers();
            
        ICharacter GetCharacter(string name);
        ICharacter GetCharacter(CommandParameter parameter);

        IReadOnlyCollection<IRoom> GetRooms();
        // TODO: get neighbour rooms
        // TODO: get room east/north/... from another room
    }
}
