using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Mud.Server.Input;

namespace Mud.Server
{
    public interface IWorld
    {
        IPlayer GetPlayer(string name);
        IPlayer GetPlayer(CommandParameter parameter);
        IReadOnlyCollection<IPlayer> GetPlayers();
            
        IReadOnlyCollection<IRoom> GetRooms();

        bool AddPlayer(IPlayer player);

        // TODO: remove
        ICharacter GetCharacter(CommandParameter parameter);
    }
}
