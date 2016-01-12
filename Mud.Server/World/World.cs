using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Aura;
using Mud.Server.Blueprints;
using Mud.Server.Constants;
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
            return _rooms.Where(x =>x.IsValid).ToList().AsReadOnly();
        }

        public IReadOnlyCollection<ICharacter> GetCharacters()
        {
            return _characters.Where(x =>x.IsValid).ToList().AsReadOnly();
        }

        public IReadOnlyCollection<IItem> GetItems()
        {
            return _items.Where(x =>x.IsValid).ToList().AsReadOnly();
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

        public IAura AddAura(ICharacter victim, string name, AuraModifiers modifier, int amount, AmountOperators amountOperator, int totalSeconds, bool visible)
        {
            IAura aura = new Aura.Aura(name, modifier, amount, amountOperator, totalSeconds);
            victim.AddAura(aura, visible);
            return aura;
        }

        public IPeriodicAura AddPeriodicAura(ICharacter victim, string name, ICharacter source, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks)
        {
            IPeriodicAura periodicAura = new PeriodicAura(name, PeriodicAuraTypes.Heal, source, amount, amountOperator, tickVisible, tickDelay, totalTicks);
            victim.AddPeriodicAura(periodicAura);
            return periodicAura;
        }

        public IPeriodicAura AddPeriodicAura(ICharacter victim, string name, ICharacter source, SchoolTypes school, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks)
        {
            IPeriodicAura periodicAura = new PeriodicAura(name, PeriodicAuraTypes.Damage, source, school, amount, amountOperator, tickVisible, tickDelay, totalTicks);
            victim.AddPeriodicAura(periodicAura);
            return periodicAura;
        }
        
        public void RemoveCharacter(ICharacter character)
        {
            character.StopFighting(true);

            // Remove from group if in a group + stop following
            if (character.Leader != null)
            {
                ICharacter leader = character.Leader;
                leader.StopFollower(character);
                if (leader.GroupMembers.Any(x => x == character))
                    leader.RemoveGroupMember(character);
            }

            // Search IPeriodicAura with character as Source and nullify Source
            List<ICharacter> charactersWithPeriodicAuras = _characters.Where(x => x.PeriodicAuras.Any(pe => pe.Source == character)).ToList(); // clone
            foreach (ICharacter characterWithPeriodicAuras in charactersWithPeriodicAuras)
            {
                foreach (IPeriodicAura pa in characterWithPeriodicAuras.PeriodicAuras.Where(x => x.Source == character))
                    pa.ResetSource();
            }

            // Remove periodic auras on character
            List<IPeriodicAura> periodicAuras = new List<IPeriodicAura>(character.PeriodicAuras); // clone
            foreach (IPeriodicAura pa in periodicAuras)
            {
                pa.ResetSource();
                character.RemovePeriodicAura(pa);
            }

            // Remove auras
            List<IAura> auras = new List<IAura>(character.Auras); // clone
            foreach(IAura aura in auras)
                character.RemoveAura(aura, false);
            // no need to recompute

            // Remove content
            if (character.Content.Any())
            {
                List<IItem> inventory = new List<IItem>(character.Content); // clone to be sure
                foreach(IItem item in inventory)
                    RemoveItem(item);
                List<IEquipable> equipment = new List<IEquipable>(character.Equipments.Where(x => x.Item != null).Select(x => x.Item));
                foreach (IEquipable item in equipment)
                    RemoveItem(item);
            }
            // Remove from room
            if (character.Room != null)
                character.Room.Leave(character);
            //
            character.OnRemoved();
            //
            _characters.Remove(character);
        }

        public void RemoveItem(IItem item)
        {
            item.OnRemoved();
            if (item.ContainedInto != null)
                item.ContainedInto.GetFromContainer(item);
            IEquipable equipable = item as IEquipable;
            if (equipable != null)
                equipable.ChangeEquipedBy(null);
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
