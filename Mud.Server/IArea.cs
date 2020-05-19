using System;
using System.Collections.Generic;
using Mud.Server.Blueprints.Area;

namespace Mud.Server
{
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
}
