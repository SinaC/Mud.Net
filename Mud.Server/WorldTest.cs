using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.Server
{
    public class WorldTest : IWorld
    {
        private readonly static Func<string, string, bool> StringEquals = (s, s1) => String.Equals(s, s1, StringComparison.InvariantCultureIgnoreCase);
        
        private readonly List<IAdmin> _admins;
        private readonly List<IPlayer> _players;
        private readonly List<ICharacter> _characters;
        private readonly List<IRoom> _rooms;

        #region Singleton

        private static readonly Lazy<WorldTest> Lazy = new Lazy<WorldTest>(() => new WorldTest());
        public static IWorld Instance { get { return Lazy.Value; } }

        private WorldTest()
        {
            _admins = new List<IAdmin>();
            _players = new List<IPlayer>();
            _characters = new List<ICharacter>();
            _rooms = new List<IRoom>();
        }

        #endregion

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
            return _players.FirstOrDefault(x => StringEquals(x.Name, name));
        }

        public IPlayer GetPlayer(CommandParameter parameter)
        {
            return _players.Where(x => StringEquals(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }

        public IReadOnlyCollection<IPlayer> GetPlayers()
        {
            return new ReadOnlyCollection<IPlayer>(_players);
        }

        public ICharacter GetCharacter(string name)
        {
            return _characters.FirstOrDefault(x => StringEquals(x.Name, name));
        }

        public ICharacter GetCharacter(CommandParameter parameter)
        {
            return _characters.Where(x => StringEquals(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }

        public IReadOnlyCollection<IRoom> GetRooms()
        {
            return new ReadOnlyCollection<IRoom>(_rooms);
        }

        #endregion
    }
}
