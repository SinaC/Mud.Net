using System;
using System.Collections.Generic;
using Mud.Network;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Room;

namespace Mud.Server.World
{
    public class World : IWorld
    {
        private readonly List<IAdmin> _admins;
        private readonly List<IPlayer> _players;
        private readonly List<ICharacter> _characters;
        private readonly List<IRoom> _rooms;
        private readonly List<IItem> _items;

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
            _items = new List<IItem>();
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
            Exit from2To = new Exit(String.Format("door[{0}->{1}]", from.Name, to.Name), to);
            from.Exits[(int) direction] = from2To;
            if (bidirectional)
            {
                Exit to2From = new Exit(String.Format("door[{0}->{1}]", to.Name, from.Name), from);
                to.Exits[(int)ServerOptions.Instance.ReverseDirection(direction)] = to2From;
            }
            return from2To;
        }

        public IItem AddItemContainer(Guid guid, string name, IContainer container)
        {
            IItem item = new Item.ItemContainer(guid, name, container);
            _items.Add(item);
            return item;
        }

        #region IWorld

        public IPlayer GetPlayer(string name, bool perfectMatch = false)
        {
            return FindHelpers.FindByName(_players, name, perfectMatch);
        }

        public IPlayer GetPlayer(CommandParameter parameter, bool perfectMatch = false)
        {
            return FindHelpers.FindByName(_players, parameter, perfectMatch);
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
            if (_players.Contains(player))
                return false;
            _players.Add(player);
            return true;
        }

        // TODO: remove
        public ICharacter GetCharacter(CommandParameter parameter, bool perfectMatch = false)
        {
            return FindHelpers.FindByName(_characters, parameter, perfectMatch);
        }

        public void Update() // called every pulse (every 1/4 seconds)
        {
            // TODO: see update.C:2332
        }

        #endregion
    }
}
