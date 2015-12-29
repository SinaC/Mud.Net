using System.Collections.Generic;
using Mud.Server.Input;

namespace Mud.Server
{
    public interface IWorld
    {
        IPlayer GetPlayer(string name, bool perfectMatch = false);
        IPlayer GetPlayer(CommandParameter parameter, bool perfectMatch = false);
        IReadOnlyCollection<IPlayer> GetPlayers();
        IReadOnlyCollection<IAdmin> GetAdmins();
        IReadOnlyCollection<IRoom> GetRooms();
        IReadOnlyCollection<IItem> GetItems();

        bool AddPlayer(IPlayer player);

        // TODO: remove
        ICharacter GetCharacter(CommandParameter parameter, bool perfectMatch = false);
    }
}
