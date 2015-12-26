using System.Collections.Generic;

namespace Mud.Server
{
    public interface IWorld
    {
        IPlayer GetPlayer(string name);
        IPlayer GetPlayer(CommandParameter parameter);
        IReadOnlyCollection<IPlayer> GetPlayers();
            
        IReadOnlyCollection<IRoom> GetRooms();

        // TODO: remove
        ICharacter GetCharacter(CommandParameter parameter);
    }
}
