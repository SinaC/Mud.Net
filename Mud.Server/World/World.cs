using System;
using System.Collections.Generic;
using Mud.Server.Blueprints;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;
using Mud.Server.Room;
using Mud.Server.Server;

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
        //public IAdmin AddAdmin(IClient client, Guid guid, string name)
        //{
        //    IAdmin admin = new Admin.Admin(client, guid, name);
        //    _admins.Add(admin);
        //    return admin;
        //}

        //public IPlayer AddPlayer(IClient client, Guid guid, string name)
        //{
        //    IPlayer player = new Player.Player(client, guid, name);
        //    _players.Add(player);
        //    return player;
        //}

        
        //public IRoom AddRoom(Guid guid, string name)
        //{
        //    IRoom room = new Room.Room(guid, name);
        //    _rooms.Add(room);
        //    return room;
        //}

        //public IRoom AddRoom(string name, string description)
        //{
        //    IRoom room = new Room.Room(Guid.NewGuid(), name, description);
        //    _rooms.Add(room);
        //    return room;
        //}

        //public IItem AddItemContainer(Guid guid, string name, IContainer container)
        //{
        //    IItem item = new ItemContainer(guid, name, container);
        //    _items.Add(item);
        //    return item;
        //}

        #region IWorld

        public IReadOnlyCollection<IRoom> GetRooms()
        {
            return _rooms.AsReadOnly();
        }

        public IReadOnlyCollection<ICharacter> GetCharacters()
        {
            return _characters.AsReadOnly();
        }

        public IReadOnlyCollection<IItem> GetItems()
        {
            return _items.AsReadOnly();
        }

        public IRoom AddRoom(Guid guid, RoomBlueprint blueprint)
        {
            IRoom room = new Room.Room(Guid.NewGuid(), blueprint);
            _rooms.Add(room);
            return room;
        }

        public IExit AddExit(IRoom from, IRoom to, ServerOptions.ExitDirections direction, bool bidirectional)
        {
            Exit from2To = new Exit(String.Format("door[{0}->{1}]", from.Name, to.Name), to);
            from.Exits[(int)direction] = from2To;
            if (bidirectional)
            {
                Exit to2From = new Exit(String.Format("door[{0}->{1}]", to.Name, from.Name), from);
                to.Exits[(int)ServerOptions.ReverseDirection(direction)] = to2From;
            }
            return from2To;
        }

        public ICharacter AddCharacter(Guid guid, string name, IRoom room) // Impersonated
        {
            ICharacter character = new Character.Character(guid, name, room);
            _characters.Add(character);
            return character;
        }

        public ICharacter AddCharacter(Guid guid, CharacterBlueprint blueprint, IRoom room) // Non-impersonated
        {
            ICharacter character = new Character.Character(guid, blueprint, room);
            _characters.Add(character);
            return character;
        }

        public IItem AddItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer container)
        {
            IItem item = new ItemContainer(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItem AddItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer container)
        {
            IItem item = new ItemArmor(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItem AddItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer container)
        {
            IItem item = new ItemWeapon(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItem AddItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer container)
        {
            IItem item = new ItemLight(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public void RemoveCharacter(ICharacter character, bool pull/*TODO better name*/) // see handler.C:3336
        {
            // TODO: nuke pets
            if (pull)
                ; // die_follower
            character.StopFighting(true);
            // TODO: extract all object in ICharacter
            // TODO: remove character from room
            if (!pull)
                ; // move character to hall room/graveyard
            else
            {
                _characters.Remove(character);
            }
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
