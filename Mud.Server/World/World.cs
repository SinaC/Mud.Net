using System;
using System.Collections.Generic;
using System.Linq;
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
            _characters = new List<ICharacter>();
            _rooms = new List<IRoom>();
            _items = new List<IItem>();
        }

        #endregion

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

        public IItemContainer AddItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer container)
        {
            IItemContainer item = new ItemContainer(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemArmor AddItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer container)
        {
            IItemArmor item = new ItemArmor(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemWeapon AddItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer container)
        {
            IItemWeapon item = new ItemWeapon(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemLight AddItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer container)
        {
            IItemLight item = new ItemLight(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom container, ICharacter victim)
        {
            IItemCorpse item = new ItemCorpse(guid, blueprint, container, victim);
            _items.Add(item);
            return item;
        }

        public void RemoveCharacter(ICharacter character)
        {
            character.StopFighting(true);
            // TODO: extract all object in ICharacter
            // TODO: remove character from room
            if (character.Content.Any())
            {
                List<IItem> inventory = new List<IItem>(character.Content); // clone to be sure
                foreach(IItem item in inventory)
                    RemoveItem(item);
                List<IItem> equipment = new List<IItem>(character.Equipments.Where(x => x.Item != null).Select(x => x.Item));
                foreach (IItem item in equipment)
                    RemoveItem(item);
            }
            if (character.Room != null)
                character.Room.Leave(character);
            character.OnRemoved();
            _characters.Remove(character);
        }

        public void RemoveItem(IItem item)
        {
            item.OnRemoved();
            _items.Remove(item);
            // If container, remove content
            IContainer container = item as IContainer;
            if (container != null)
            {
                List<IItem> content = new List<IItem>(container.Content); // clone to be sure
                foreach (IItem itemInContainer in content)
                    RemoveItem(itemInContainer);
            }
        }

        public void RemoveRoom(IRoom room)
        {
            // TODO
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
