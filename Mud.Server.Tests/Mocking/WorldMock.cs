using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mud.Server.Tests.Mocking
{
    public class WorldMock : IWorld
    {
        private readonly List<IAdmin> _admins;
        private readonly List<IPlayer> _players;
        private readonly List<ICharacter> _characters;
        private readonly List<IRoom> _rooms;

        public WorldMock()
        {
            _admins = new List<IAdmin>();
            _players = new List<IPlayer>();
            _characters = new List<ICharacter>();
            _rooms = new List<IRoom>();
        }

        public IAdmin AddAdmin(Guid guid, string name)
        {
            IAdmin admin = new Admin.Admin(guid, name);
            _admins.Add(admin);
            return admin;
        }

        public IPlayer AddPlayer(Guid guid, string name)
        {
            IPlayer player = new Player.Player(guid, name);
            _players.Add(player);
            return player;
        }

        public ICharacter AddCharacter(Guid guid, string name, IRoom room)
        {
            ICharacter character = new Character.Character(guid, name, room);
            _characters.Add(character);
            return character;
        }

        public IRoom AddRoom(Guid guid, string name)
        {
            IRoom room = new Room.Room(guid, name);
            _rooms.Add(room);
            return room;
        }

        #region IWorld

        public IPlayer GetPlayer(string name)
        {
            return FindHelpers.FindByName(_players, name);
        }

        public IPlayer GetPlayer(CommandParameter parameter)
        {
            return FindHelpers.FindByName(_players, parameter);
        }

        public IReadOnlyCollection<IPlayer> GetPlayers()
        {
            return new ReadOnlyCollection<IPlayer>(_players);
        }

        public IReadOnlyCollection<IRoom> GetRooms()
        {
            return new ReadOnlyCollection<IRoom>(_rooms);
        }

        public ICharacter GetCharacter(CommandParameter parameter)
        {
            return FindHelpers.FindByName(_characters, parameter);
        }

        #endregion
    }
}
