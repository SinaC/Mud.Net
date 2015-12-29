using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mud.Network;
using Mud.Server.Input;

namespace Mud.Server.Tests.Mocking
{
    public class WorldMock : IWorld
    {
        private readonly List<IAdmin> _admins;
        private readonly List<IPlayer> _players;
        private readonly List<ICharacter> _characters;
        private readonly List<IRoom> _rooms;
        private readonly List<IObject> _objects;

        public WorldMock()
        {
            _admins = new List<IAdmin>();
            _players = new List<IPlayer>();
            _characters = new List<ICharacter>();
            _rooms = new List<IRoom>();
            _objects = new List<IObject>();
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
            return new ReadOnlyCollection<IPlayer>(_players);
        }

        public IReadOnlyCollection<IAdmin> GetAdmins()
        {
            return new ReadOnlyCollection<IAdmin>(_admins);
        }

        public IReadOnlyCollection<IRoom> GetRooms()
        {
            return new ReadOnlyCollection<IRoom>(_rooms);
        }

        public IReadOnlyCollection<IObject> GetObjects()
        {
            return new ReadOnlyCollection<IObject>(_objects);
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
