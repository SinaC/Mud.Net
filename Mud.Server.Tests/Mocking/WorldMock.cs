using System;
using System.Collections.Generic;
using Mud.Network;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Tests.Mocking
{
    public class WorldMock : IWorld
    {
        private readonly List<IAdmin> _admins;
        private readonly List<IPlayer> _players;
        private readonly List<ICharacter> _characters;
        private readonly List<IRoom> _rooms;
        private readonly List<IItem> _items;

        public WorldMock()
        {
            _admins = new List<IAdmin>();
            _players = new List<IPlayer>();
            _characters = new List<ICharacter>();
            _rooms = new List<IRoom>();
            _items = new List<IItem>();
        }

        public IAdmin AddAdmin(IClient client, Guid guid, string name)
        {
            IAdmin admin = new Admin.Admin(client, guid, name);
            _admins.Add(admin);
            return admin;
        }

        public IPlayer AddPlayer(IClient client, Guid guid, string name)
        {
            IPlayer player = new Player.Player(client, guid, name);
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

        public IPlayer GetPlayer(string name, bool perfectMatch = false)
        {
            return FindHelpers.FindByName(_players, name);
        }

        public IPlayer GetPlayer(CommandParameter parameter, bool perfectMatch = false)
        {
            return FindHelpers.FindByName(_players, parameter);
        }

        public IReadOnlyCollection<IPlayer> GetPlayers()
        {
            return _players.AsReadOnly();
        }

        public IReadOnlyCollection<IAdmin> GetAdmins()
        {
            return _admins.AsReadOnly();
        }

        public IReadOnlyCollection<IRoom> GetRooms()
        {
            return _rooms.AsReadOnly();
        }

        public IReadOnlyCollection<IItem> GetItems()
        {
            return _items.AsReadOnly();
        }

        public bool AddPlayer(IPlayer player)
        {
            _players.Add(player);
            return true;
        }

        public ICharacter GetCharacter(CommandParameter parameter, bool perfectMatch = false)
        {
            return FindHelpers.FindByName(_characters, parameter);
        }

        #endregion
    }
}
