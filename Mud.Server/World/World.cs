using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Logger;
using Mud.Server.Aura;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Constants;
using Mud.Server.Item;
using Mud.Server.Room;

namespace Mud.Server.World
{
    public class World : IWorld
    {
        private readonly List<TreasureTable<int>> _treasureTables;
        private readonly Dictionary<int, QuestBlueprint> _questBlueprints;
        private readonly Dictionary<int, RoomBlueprint> _roomBlueprints;
        private readonly Dictionary<int, CharacterBlueprint> _characterBlueprints;
        private readonly Dictionary<int, ItemBlueprintBase> _itemBlueprints;
        private readonly List<IArea> _areas;
        private readonly List<IRoom> _rooms;
        private readonly List<ICharacter> _characters;
        private readonly List<IItem> _items;

        #region Singleton

        private static readonly Lazy<World> Lazy = new Lazy<World>(() => new World());

        public static IWorld Instance => Lazy.Value;

        private World()
        {
            _treasureTables = new List<TreasureTable<int>>();
            _questBlueprints = new Dictionary<int, QuestBlueprint>();
            _roomBlueprints = new Dictionary<int, RoomBlueprint>();
            _characterBlueprints = new Dictionary<int, CharacterBlueprint>();
            _itemBlueprints = new Dictionary<int, ItemBlueprintBase>();
            _areas = new List<IArea>();
            _rooms = new List<IRoom>();
            _characters = new List<ICharacter>();
            _items = new List<IItem>();
        }

        #endregion

        #region IWorld

        // Treasures
        public IReadOnlyCollection<TreasureTable<int>> TreasureTables => _treasureTables;

        public void AddTreasureTable(TreasureTable<int> table)
        {
            // TODO: check if already exists ?
            _treasureTables.Add(table);
        }

        // Blueprints
        public IReadOnlyCollection<QuestBlueprint> QuestBlueprints => _questBlueprints.Values.ToList().AsReadOnly();

        public IReadOnlyCollection<RoomBlueprint> RoomBlueprints => _roomBlueprints.Values.ToList().AsReadOnly();

        public IReadOnlyCollection<CharacterBlueprint> CharacterBlueprints => _characterBlueprints.Values.ToList().AsReadOnly();

        public IReadOnlyCollection<ItemBlueprintBase> ItemBlueprints => _itemBlueprints.Values.ToList().AsReadOnly();

        public QuestBlueprint GetQuestBlueprint(int id)
        {
            return GetBlueprintById(_questBlueprints, id);
        }

        public RoomBlueprint GetRoomBlueprint(int id)
        {
            return GetBlueprintById(_roomBlueprints, id);
        }

        public CharacterBlueprint GetCharacterBlueprint(int id)
        {
            return GetBlueprintById(_characterBlueprints, id);
        }

        public ItemBlueprintBase GetItemBlueprint(int id)
        {
            return GetBlueprintById(_itemBlueprints, id);
        }

        public void AddQuestBlueprint(QuestBlueprint blueprint)
        {
            if (_questBlueprints.ContainsKey(blueprint.Id))
                Log.Default.WriteLine(LogLevels.Error, $"Quest blueprint duplicate {blueprint.Id}!!!");
            else
                _questBlueprints.Add(blueprint.Id, blueprint);
        }

        public void AddRoomBlueprint(RoomBlueprint blueprint)
        {
            if (_roomBlueprints.ContainsKey(blueprint.Id))
                Log.Default.WriteLine(LogLevels.Error, $"Room blueprint duplicate {blueprint.Id}!!!");
            else
                _roomBlueprints.Add(blueprint.Id, blueprint);
        }

        public void AddCharacterBlueprint(CharacterBlueprint blueprint)
        {
            if (_characterBlueprints.ContainsKey(blueprint.Id))
                Log.Default.WriteLine(LogLevels.Error, $"Character blueprint duplicate {blueprint.Id}!!!");
            else
                _characterBlueprints.Add(blueprint.Id, blueprint);
        }

        public void AddItemBlueprint(ItemBlueprintBase blueprint)
        {
            if (_itemBlueprints.ContainsKey(blueprint.Id))
                Log.Default.WriteLine(LogLevels.Error, $"Item blueprint duplicate {blueprint.Id}!!!");
            else
                _itemBlueprints.Add(blueprint.Id, blueprint);
        }

        //
        public IEnumerable<IArea> Areas => _areas;

        public IEnumerable<IRoom> Rooms => _rooms.Where(x => x.IsValid);

        public IEnumerable<ICharacter> Characters => _characters.Where(x => x.IsValid);

        public IEnumerable<IItem> Items => _items.Where(x => x.IsValid);

        public IArea AddArea(Guid guid, string displayName, int minLevel, int maxLevel, string builders, string credits)
        {
            IArea area = new Area.Area(displayName, minLevel, maxLevel, builders, credits);
            _areas.Add(area);
            return area;
        }

        public IRoom AddRoom(Guid guid, RoomBlueprint blueprint, IArea area)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IRoom room = new Room.Room(Guid.NewGuid(), blueprint, area);
            _rooms.Add(room);
            return room;
        }

        public IExit AddExit(IRoom from, IRoom to, ExitBlueprint blueprint, ExitDirections direction)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            Exit from2To = new Exit(blueprint, to);
            from.Exits[(int)direction] = from2To;
            return from2To;
        }

        public ICharacter AddCharacter(Guid guid, string name, IClass pcClass, IRace pcRace, Sex pcSex, IRoom room) // PC
        {
            ICharacter character = new Character.Character(guid, name, pcClass, pcRace, pcSex, room);
            _characters.Add(character);
            return character;
        }

        public ICharacter AddCharacter(Guid guid, CharacterBlueprint blueprint, IRoom room) // NPC
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            ICharacter character = new Character.Character(guid, blueprint, room);
            _characters.Add(character);
            return character;
        }

        public IItemContainer AddItemContainer(Guid guid, ItemContainerBlueprint blueprint, IContainer container)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IItemContainer item = new ItemContainer(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemArmor AddItemArmor(Guid guid, ItemArmorBlueprint blueprint, IContainer container)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IItemArmor item = new ItemArmor(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemWeapon AddItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer container)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IItemWeapon item = new ItemWeapon(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemLight AddItemLight(Guid guid, ItemLightBlueprint blueprint, IContainer container)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IItemLight item = new ItemLight(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemCorpse AddItemCorpse(Guid guid, ItemCorpseBlueprint blueprint, IRoom room, ICharacter victim, ICharacter killer)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IItemCorpse item = new ItemCorpse(guid, blueprint, room, victim, killer);
            _items.Add(item);
            return item;
        }

        public IItemShield AddItemShield(Guid guid, ItemShieldBlueprint blueprint, IContainer container)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IItemShield item = new ItemShield(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemFurniture AddItemFurniture(Guid guid, ItemFurnitureBlueprint blueprint, IContainer container)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IItemFurniture item = new ItemFurniture(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemJewelry AddItemJewelry(Guid guid, ItemJewelryBlueprint blueprint, IContainer container)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IItemJewelry item = new ItemJewelry(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemQuest AddItemQuest(Guid guid, ItemQuestBlueprint blueprint, IContainer container)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IItemQuest item = new ItemQuest(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItemKey AddItemKey(Guid guid, ItemKeyBlueprint blueprint, IContainer container)
        {
            if (blueprint == null)
                throw new ArgumentNullException(nameof(blueprint));
            IItemKey item = new ItemKey(guid, blueprint, container);
            _items.Add(item);
            return item;
        }

        public IItem AddItem(Guid guid, int blueprintId, IContainer container)
        {
            ItemBlueprintBase blueprint = GetItemBlueprint(blueprintId);
            if (blueprint == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "World.AddItem: unknown blueprintId {0}", blueprintId);
                return null;
            }
            var weaponBlueprint = blueprint as ItemWeaponBlueprint;
            if (weaponBlueprint != null)
                return AddItemWeapon(guid, weaponBlueprint, container);
            var containerBlueprint = blueprint as ItemContainerBlueprint;
            if (containerBlueprint != null)
                return AddItemContainer(guid, containerBlueprint, container);
            var armorBlueprint = blueprint as ItemArmorBlueprint;
            if (armorBlueprint != null)
                return AddItemArmor(guid, armorBlueprint, container);
            var lightBlueprint = blueprint as ItemLightBlueprint;
            if (lightBlueprint != null)
                return AddItemLight(guid, lightBlueprint, container);
            var furnitureBlueprint = blueprint as ItemFurnitureBlueprint;
            if (furnitureBlueprint != null)
                return AddItemFurniture(guid, furnitureBlueprint, container);
            var jewelryBlueprint = blueprint as ItemJewelryBlueprint;
            if (jewelryBlueprint != null)
                return AddItemJewelry(guid, jewelryBlueprint, container);
            var shieldBlueprint = blueprint as ItemShieldBlueprint;
            if (shieldBlueprint != null)
                return AddItemShield(guid, shieldBlueprint, container);
            var keyBlueprint = blueprint as ItemKeyBlueprint;
            if (keyBlueprint != null)
                return AddItemKey(guid, keyBlueprint, container);
            Log.Default.WriteLine(LogLevels.Error, $"World.AddItem: Unknown blueprint id or type {blueprintId} {blueprint.GetType().FullName}");
            // TODO: other blueprint
            return null;
        }

        public IAura AddAura(ICharacter victim, IAbility ability, ICharacter source, AuraModifiers modifier, int amount, AmountOperators amountOperator, int totalSeconds, bool recompute)
        {
            IAura aura = new Aura.Aura(ability, source, modifier, amount, amountOperator, totalSeconds);
            victim.AddAura(aura, recompute);
            return aura;
        }

        public IPeriodicAura AddPeriodicAura(ICharacter victim, IAbility ability, ICharacter source, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks)
        {
            IPeriodicAura periodicAura = new PeriodicAura(ability, PeriodicAuraTypes.Heal, source, amount, amountOperator, tickVisible, tickDelay, totalTicks);
            victim.AddPeriodicAura(periodicAura);
            return periodicAura;
        }

        public IPeriodicAura AddPeriodicAura(ICharacter victim, IAbility ability, ICharacter source, SchoolTypes school, int amount, AmountOperators amountOperator, bool tickVisible, int tickDelay, int totalTicks)
        {
            IPeriodicAura periodicAura = new PeriodicAura(ability, PeriodicAuraTypes.Damage, source, school, amount, amountOperator, tickVisible, tickDelay, totalTicks);
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
                    leader.RemoveGroupMember(character, false);
            }
            // TODO: if leader of a group

            // Search IPeriodicAura with character as Source and nullify Source
            List<ICharacter> charactersWithPeriodicAuras = _characters.Where(x => x.PeriodicAuras.Any(pa => pa.Source == character)).ToList(); // clone
            foreach (ICharacter characterWithPeriodicAuras in charactersWithPeriodicAuras)
            {
                foreach (IPeriodicAura pa in characterWithPeriodicAuras.PeriodicAuras.Where(x => x.Source == character))
                    pa.ResetSource();
            }

            // Search IAura with character as Source and nullify Source
            List<ICharacter> charactersWithAuras = _characters.Where(x => x.Auras.Any(a => a.Source == character)).ToList(); // clone
            foreach (ICharacter characterWithAuras in charactersWithAuras)
            {
                foreach (IAura aura in characterWithAuras.Auras.Where(x => x.Source == character))
                    aura.ResetSource();
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
            foreach (IAura aura in auras)
            {
                aura.ResetSource();
                character.RemoveAura(aura, false);
            }
            // no need to recompute

            // Remove content
            if (character.Content.Any())
            {
                List<IItem> clonedInventory = new List<IItem>(character.Content); // clone because GetFromContainer change Content collection
                foreach (IItem item in clonedInventory)
                    RemoveItem(item);
                // Remove equipments
                if (character.Equipments.Any(x => x.Item != null))
                {
                    List<IEquipable> equipment = new List<IEquipable>(character.Equipments.Where(x => x.Item != null).Select(x => x.Item));
                    foreach (IEquipable item in equipment)
                        RemoveItem(item);
                }
            }
            // Remove from room
            character.ChangeRoom(null);
            //
            character.OnRemoved();
            //_characters.Remove(character); will removed in cleanup step
        }

        public void RemoveItem(IItem item)
        {
            //item.ContainedInto?.GetFromContainer(item);
            item.ChangeContainer(null);
            IEquipable equipable = item as IEquipable;
            equipable?.ChangeEquipedBy(null);
            // If container, remove content
            IContainer container = item as IContainer;
            if (container != null)
            {
                List<IItem> content = new List<IItem>(container.Content); // clone to be sure
                foreach (IItem itemInContainer in content)
                    RemoveItem(itemInContainer);
            }
            item.OnRemoved();
            //_items.Remove(item); will removed in cleanup step
        }

        public void RemoveRoom(IRoom room)
        {
            throw new NotImplementedException();
        }

        public void Update() // called every pulse (every 1/4 seconds)
        {
            // TODO: see update.C:2332
        }

        public void Cleanup() // remove invalid entities
        {
            // TODO: room ?
            if (_items.Any(x => !x.IsValid))
                Log.Default.WriteLine(LogLevels.Debug, $"Cleaning {_items.Count(x => !x.IsValid)} item(s)");
            if (_characters.Any(x => !x.IsValid))
                Log.Default.WriteLine(LogLevels.Debug, $"Cleaning {_characters.Count(x => !x.IsValid)} character(s)");

            _items.RemoveAll(x => !x.IsValid);
            _characters.RemoveAll(x => !x.IsValid);
            // TODO: room
        }

        #endregion

        private T GetBlueprintById<T>(IDictionary<int, T> blueprints, int id)
        {
            T blueprint;
            blueprints.TryGetValue(id, out blueprint);
            return blueprint;
        }
    }
}
