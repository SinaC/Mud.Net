using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mud.Network;
using Mud.Server.Input;
using Mud.Server.Room;

namespace Mud.Server
{
    public class World : IWorld
    {
        private readonly List<IAdmin> _admins;
        private readonly List<IPlayer> _players;
        private readonly List<ICharacter> _characters;
        private readonly List<IRoom> _rooms;

        #region Singleton

        private static readonly Lazy<World> Lazy = new Lazy<World>(() => new World());

        public static IWorld Instance
        {
            get { return Lazy.Value; }
        }

        private World()
        {
            _admins = new List<IAdmin>();
            _players = new List<IPlayer>();
            _characters = new List<ICharacter>();
            _rooms = new List<IRoom>();
        }

        #endregion

        // TODO: remove following methods
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

        public IExit AddExit(IRoom from, IRoom to, ServerOptions.ExitDirections direction, bool bidirectional)
        {
            Exit from2To = new Exit("door", to);
            from.Exits[(int) direction] = from2To;
            if (bidirectional)
            {
                Exit to2From = new Exit("door", from);
                to.Exits[(int)ServerOptions.Instance.ReverseDirection(direction)] = to2From;
            }
            return from2To;
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

        public bool AddPlayer(IPlayer player)
        {
            if (_players.Contains(player))
                return false;
            _players.Add(player);
            return true;
        }

        // TODO: remove
        public ICharacter GetCharacter(CommandParameter parameter)
        {
            return FindHelpers.FindByName(_characters, parameter);
        }

        #endregion
    }
}
