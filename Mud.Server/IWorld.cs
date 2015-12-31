using System.Collections.Generic;
using Mud.Server.Input;

namespace Mud.Server
{
    public interface IWorld
    {
        IReadOnlyCollection<IRoom> GetRooms();
        IReadOnlyCollection<ICharacter> GetCharacters();
        IReadOnlyCollection<IItem> GetItems();

        // TODO: remove
        // TEST PURPOSE
        ICharacter GetCharacter(CommandParameter parameter, bool perfectMatch = false);

        void Update(); // called every pulse
    }
}
